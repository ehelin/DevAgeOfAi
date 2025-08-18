from transformers import AutoTokenizer, AutoModelForCausalLM
from peft import PeftModel
import sys
import torch
import os

# Check if fine-tuned model exists
FINE_TUNED_PATH = "./fine_tuned_phi_habits"
USE_FINE_TUNED = os.path.exists(os.path.join(FINE_TUNED_PATH, "adapter_config.json"))

# Load the tokenizer and model
model_name = "microsoft/Phi-3.5-mini-instruct"
MODEL_REVISION = "main"  # Pin to specific revision for security

if USE_FINE_TUNED:
    print(f"Loading fine-tuned model from {FINE_TUNED_PATH}...", file=sys.stderr)
    # Load base model with security settings
    base_model = AutoModelForCausalLM.from_pretrained(
        model_name,
        revision=MODEL_REVISION,
        trust_remote_code=True,  # Required for Phi-3.5
        torch_dtype=torch.bfloat16 if torch.cuda.is_available() else torch.float32,
        device_map="auto",
        attn_implementation="eager"  # Fix DynamicCache compatibility
    )
    # Load LoRA weights on top
    model = PeftModel.from_pretrained(base_model, FINE_TUNED_PATH)
    tokenizer = AutoTokenizer.from_pretrained(FINE_TUNED_PATH)
    print("Fine-tuned model loaded successfully!", file=sys.stderr)
else:
    print(f"Loading base model {model_name}...", file=sys.stderr)
    print(f"Revision: {MODEL_REVISION}", file=sys.stderr)
    tokenizer = AutoTokenizer.from_pretrained(
        model_name,
        revision=MODEL_REVISION,
        trust_remote_code=True  # Required for Phi-3.5
    )
    model = AutoModelForCausalLM.from_pretrained(
        model_name,
        revision=MODEL_REVISION,
        trust_remote_code=True,  # Required for Phi-3.5
        torch_dtype=torch.bfloat16 if torch.cuda.is_available() else torch.float32,
        device_map="auto",
        attn_implementation="eager"  # Fix DynamicCache compatibility
    )
    print("Note: Fine-tuned model not found. Run improved_qlora_train.py first for better results.", file=sys.stderr)

def generate_response(input_line):
    try:
        if USE_FINE_TUNED:
            # Fine-tuned model was trained on this exact format
            prompt = f"User: {input_line}\nAssistant:"
        else:
            # Base model needs chat template
            messages = [
                {
                    "role": "system", 
                    "content": "You are a habit tracker. Reply with ONLY a 5-6 word habit. Examples: 'Drink eight glasses of water daily', 'Walk ten thousand steps daily', 'Read twenty pages before bed'. No explanations, no extra text."
                },
                {
                    "role": "user", 
                    "content": input_line
                }
            ]
            prompt = tokenizer.apply_chat_template(
                messages, 
                tokenize=False, 
                add_generation_prompt=True
            )
        
        # Encode the input text
        inputs = tokenizer(prompt, return_tensors="pt")
        
        # Ensure model is running on the correct device
        inputs = {k: v.to(model.device) for k, v in inputs.items()}
            
        # Generate response
        outputs = model.generate(
            **inputs,
            max_new_tokens=15,  # Keep it short
            do_sample=True,
            temperature=0.7,
            top_p=0.9,
            pad_token_id=tokenizer.pad_token_id,
            eos_token_id=tokenizer.eos_token_id,
        )

        # Decode the response
        full_response = tokenizer.decode(outputs[0], skip_special_tokens=True)
        
        # Extract only the new generated text
        if USE_FINE_TUNED:
            # For fine-tuned model, remove the prompt we added
            response = full_response[len(prompt):].strip()
        else:
            # For base model, extract assistant response
            if "<|assistant|>" in full_response:
                response = full_response.split("<|assistant|>")[-1].strip()
            else:
                response = full_response[len(prompt):].strip()
        
        # Clean up response - take only until end of text token or first complete thought
        if '<|endoftext|>' in response:
            response = response.split('<|endoftext|>')[0].strip()
        
        # Take only first line/sentence for clean output  
        response = response.split('\n')[0].strip()
        response = response.split('.')[0].strip() if '.' in response else response
        
        # Ensure single-line output
        response = " ".join(response.splitlines()).strip()

        # Only show debug in non-interactive mode
        if not sys.stdin.isatty():
            print(f"DEBUG: Generated Response: {response}", file=sys.stderr)
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

            # Ignore empty input
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
    print("Starting Python script...", file=sys.stderr)

    if sys.stdin.isatty():  # Running in CLI mode
        status = "FINE-TUNED MODEL" if USE_FINE_TUNED else "BASE MODEL"
        print(f"Running in interactive mode with {status}. Type 'exit' to quit.")
        print("\nTip: Try asking 'Please suggest a habit that can be tracked'\n")
        
        while True:
            input_text = input("You: ")
            if input_text.lower() == "exit":
                print("Exiting interactive mode.")
                break
            
            response = generate_response(input_text)
            print(f"Model: {response}")

    else:  # Running inside C# service
        print("Running in main mode (C# integration).", file=sys.stderr)
        sys.stdout.flush()
        main()