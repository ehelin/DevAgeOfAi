from transformers import AutoTokenizer, AutoModelForCausalLM
import sys
import torch

# Load the tokenizer and model from Hugging Face
model_name = "microsoft/Phi-3.5-mini-instruct"
tokenizer = AutoTokenizer.from_pretrained(model_name)
model = AutoModelForCausalLM.from_pretrained(
    model_name,
    torch_dtype=torch.bfloat16 if torch.cuda.is_available() else torch.float32,
    device_map="auto"
)

def generate_response(input_line):
    try:
        # Create a proper chat template for Phi-3.5
        messages = [
            {
                "role": "system", 
                "content": "You are a habit tracking assistant. When asked for habit suggestions, provide specific, actionable, trackable habits. Examples: 'Walk 10000 steps daily', 'Drink 8 glasses of water', 'Read for 30 minutes before bed', 'Meditate for 10 minutes each morning'. Be concise and provide only the habit without explanation."
            },
            {
                "role": "user", 
                "content": input_line
            }
        ]
        
        # Apply the chat template
        prompt = tokenizer.apply_chat_template(
            messages, 
            tokenize=False, 
            add_generation_prompt=True
        )
        
        # Encode the input text
        inputs = tokenizer(prompt, return_tensors="pt")
        
        # Ensure model is running on the correct device (CPU/GPU)
        inputs = {k: v.to(model.device) for k, v in inputs.items()}
            
        # Generate a response with optimized parameters for habit generation
        outputs = model.generate(
            **inputs,
            max_new_tokens=30,  # Habits are short
            do_sample=True,      # Enable sampling for variety
            temperature=0.7,     # Balanced creativity
            top_p=0.9,          # Nucleus sampling
            pad_token_id=tokenizer.pad_token_id,
            eos_token_id=tokenizer.eos_token_id,
        )

        # Decode the response
        full_response = tokenizer.decode(outputs[0], skip_special_tokens=True)
        
        # Extract only the assistant's response
        if "<|assistant|>" in full_response:
            response = full_response.split("<|assistant|>")[-1].strip()
        else:
            # Fallback: remove the input prompt from response
            response = full_response[len(prompt):].strip()
        
        # Clean up response - remove any system message if it leaked
        if response.startswith("You are a habit"):
            # Find where the actual habit starts
            lines = response.split(".")
            for i, line in enumerate(lines):
                if any(keyword in line.lower() for keyword in ["walk", "drink", "read", "exercise", "meditate", "sleep", "eat", "write", "study", "practice"]):
                    response = ".".join(lines[i:]).strip()
                    break
        
        # Ensure single-line output
        response = " ".join(response.splitlines()).strip()
        
        print(f"DEBUG: Generated Response: {response}", file=sys.stderr)  # Debug output
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