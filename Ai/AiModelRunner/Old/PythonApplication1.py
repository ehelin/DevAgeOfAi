from transformers import AutoTokenizer, AutoModelForCausalLM
import sys
import torch

# Load the tokenizer and model from Hugging Face
model_name = "microsoft/Phi-3.5-mini-instruct"
tokenizer = AutoTokenizer.from_pretrained(model_name)
model = AutoModelForCausalLM.from_pretrained(model_name)

def generate_response(input_line):
    try:
        promptWithInput = input_line

        # Encode the input text
        inputs = tokenizer(promptWithInput, return_tensors="pt")

        # Ensure model is running on the correct device (CPU/GPU)
        inputs = {k: v.to(model.device) for k, v in inputs.items()}
            
        # Generate a response with adjusted parameters
        outputs = model.generate(
            **inputs,
            max_new_tokens=30,
            num_beams=5,  # Ensures structured output
            do_sample=True,  # Enables sampling to avoid repetition
            temperature=0.7,  # Adds slight randomness
            top_p=0.9  # Nucleus sampling
        )

        # Decode and format response to ensure single-line output
        if outputs.size(0) > 0:
            response = tokenizer.decode(outputs[0], skip_special_tokens=True).strip()
            response = response[len(promptWithInput):].strip()
            response = " ".join(response.splitlines())  # Flatten response into one line

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
