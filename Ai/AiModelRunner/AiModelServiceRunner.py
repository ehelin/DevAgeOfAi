import sys
import os
import warnings
import logging

# Suppress all warnings
warnings.filterwarnings('ignore')
os.environ['TF_CPP_MIN_LOG_LEVEL'] = '3'

# Suppress transformers logging
logging.getLogger('transformers').setLevel(logging.ERROR)
logging.getLogger('transformers.modeling_utils').setLevel(logging.ERROR)
logging.getLogger('transformers.tokenization_utils').setLevel(logging.ERROR)

# Now import transformers after setting up suppression
from transformers import AutoTokenizer, AutoModelForCausalLM
from peft import PeftModel
import torch

# Disable transformers warnings about deprecations
import transformers
transformers.logging.set_verbosity_error()

# Model configuration
model_name = "microsoft/Phi-3.5-mini-instruct"
adapter_path = "./fine_tuned_phi_habits"  # Path to your fine-tuned adapters

# Load the tokenizer
with warnings.catch_warnings():
    warnings.simplefilter("ignore")
    tokenizer = AutoTokenizer.from_pretrained(model_name)

# Load the base model
# SECURITY: Always scan for malicious code when loading models
with warnings.catch_warnings():
    warnings.simplefilter("ignore")
    # Redirect stderr temporarily to suppress loading bars
    import contextlib
    import io
    with contextlib.redirect_stderr(io.StringIO()):
        model = AutoModelForCausalLM.from_pretrained(
            model_name,
            torch_dtype=torch.bfloat16 if torch.cuda.is_available() else torch.float32,
            device_map="auto",
            trust_remote_code=True,  # Required for Phi-3.5
            code_revision=None  # Always use latest code revision with security patches
        )

# Load the fine-tuned LoRA adapters if they exist
if os.path.exists(adapter_path):
    with warnings.catch_warnings():
        warnings.simplefilter("ignore")
        model = PeftModel.from_pretrained(model, adapter_path)

def generate_response(input_line):
    try:
        # For the fine-tuned model, we use the exact prompt it was trained on
        if "suggest a habit" in input_line.lower():
            # Use the exact format from training
            messages = [
                {"role": "user", "content": input_line}
            ]
        else:
            # For other queries, add a system message
            messages = [
                {
                    "role": "system", 
                    "content": "You are a helpful habit tracking assistant. Provide concise, actionable responses."
                },
                {"role": "user", "content": input_line}
            ]
        
        # Apply the chat template
        prompt = tokenizer.apply_chat_template(
            messages, 
            tokenize=False, 
            add_generation_prompt=True
        )
        
        # Encode the input text
        inputs = tokenizer(prompt, return_tensors="pt", truncation=True, max_length=512)
        
        # Ensure model is running on the correct device (CPU/GPU)
        inputs = {k: v.to(model.device) for k, v in inputs.items()}
            
        # Generate a response with parameters optimized for the fine-tuned model
        with warnings.catch_warnings():
            warnings.simplefilter("ignore")
            outputs = model.generate(
                **inputs,
                max_new_tokens=10,  # Enough for 5-6 word habits
                do_sample=True,     # Enable sampling for variety
                temperature=0.5,    # Moderate temperature for good variety
                top_p=0.9,          # Standard nucleus sampling
                pad_token_id=tokenizer.pad_token_id,
                eos_token_id=tokenizer.eos_token_id
            )

        # Decode only the generated tokens (exclude input)
        generated_tokens = outputs[0][len(inputs['input_ids'][0]):]
        response = tokenizer.decode(generated_tokens, skip_special_tokens=True).strip()
        
        # Clean up the response
        if response:
            # Remove any quotation marks if present
            response = response.strip('"\'')
            
            # Take only the first line if there are multiple
            if '\n' in response:
                response = response.split('\n')[0].strip()
            
            # Clean up any incomplete sentences
            # If it doesn't end with punctuation and has more than 3 words, add a period
            if response and not response[-1] in '.!?':
                words = response.split()
                if len(words) >= 3:
                    response = response + '.'
            
            response = response.strip()
        
        return response

    except Exception as e:
        return f"An error occurred: {str(e)}"

def main():
    """Handles interaction with C# via stdin/stdout."""
    print("Python model ready")  # Signal to C# that Python is ready
    sys.stdout.flush()

    while True:
        try:
            input_line = sys.stdin.readline().strip()

            # Ignore empty input (prevents hanging)
            if not input_line:
                continue  

            # Exit condition
            if input_line.lower() == "exit":
                print("Python exiting")
                sys.stdout.flush()
                break

            # Generate response
            response = generate_response(input_line)

            # Print response for C# to read
            print(response)
            sys.stdout.flush()

        except Exception as e:
            print(f"Error: {str(e)}")
            sys.stdout.flush()

if __name__ == "__main__":
    print("Starting Python script...")

    if sys.stdin.isatty():  # Running in CLI mode
        print("Running in interactive mode. Type 'exit' to quit.")
        print("\nTip: Try asking 'Please suggest a habit that can be tracked'\n")
        
        while True:
            input_text = input("You: ")
            if input_text.lower() == "exit":
                print("Exiting interactive mode.")
                break
            
            response = generate_response(input_text)
            print("Model:", response)

    else:  # Running inside C# service
        print("Running in main mode (C# integration).")
        sys.stdout.flush()
        main()