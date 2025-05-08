import sys
import io
import torch
from transformers import AutoTokenizer, AutoModelForCausalLM

# Force UTF-8 encoding for correct output handling
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8', errors='replace')
sys.stderr = io.TextIOWrapper(sys.stderr.buffer, encoding='utf-8', errors='replace')

print("DEBUG: Python script starting...", file=sys.stderr)
sys.stderr.flush()

try:
    print("DEBUG: Loading model and tokenizer...", file=sys.stderr)
    sys.stderr.flush()

    # Load the tokenizer and model from Hugging Face
    model_name = "microsoft/Phi-3.5-mini-instruct"
    tokenizer = AutoTokenizer.from_pretrained(model_name)
    model = AutoModelForCausalLM.from_pretrained(model_name)

    print("Python model ready")  # Signal to C# that Python is ready
    sys.stdout.flush()

except Exception as e:
    print(f"ERROR: Failed to load model: {str(e)}", file=sys.stderr)
    sys.stderr.flush()
    sys.exit(1)

def filter_output(response, promptWithInput):
    # # Ensure "Habit:" prefix is always present
    # if not response.startswith("Habit:"):
    #     response = f"Habit: {response}"
        
    response = response[len(promptWithInput):].strip()  # Remove prompt from script
    response = filter_output_character(response, "[")
    response = filter_output_character(response, ",")
    response = filter_output_character(response, "-")
    response = filter_output_character(response, "Γ")

    response = " ".join(response.splitlines()).strip()  # Flatten response

    return response

def filter_output_character(response, character):
    index = response.find(character)  # Use find() instead of index()
    if index != -1:  # Only slice if "[" exists
        response = response[:index]

    return response

def generate_response():
    try:
        promptWithInput = "Provide a habit to track only without description.\nHabit:"  # Enforce "Habit: " format

        # Encode the input text
        inputs = tokenizer(promptWithInput, return_tensors="pt")

        # Ensure model is running on the correct device (CPU/GPU)
        inputs = {k: v.to(model.device) for k, v in inputs.items()}

        # Generate a response with optimized parameters
        outputs = model.generate(
            **inputs,
            max_new_tokens=30,  # Keep response concise
            num_beams=1,  # Disable beam search
            do_sample=True,  # Enable randomness
            temperature=0.7,  # Reduce randomness for more predictable outputs
            top_p=0.85,  # Ensure diversity
            top_k=40,  # Limit vocabulary scope
            repetition_penalty=1.5,  # Prevent redundant words
            pad_token_id=tokenizer.eos_token_id  # Prevents unfinished text artifacts
        )

        # Decode and format response to ensure single-line output
        response = tokenizer.decode(outputs[0], skip_special_tokens=True).strip()
        response = filter_output(response, promptWithInput)

        # print(f"DEBUG: Generated Response: {response}", file=sys.stderr)  # Debug output
        return response

    except Exception as e:
        print(f"ERROR: Failed to generate response: {str(e)}", file=sys.stderr)
        sys.stderr.flush()
        return f"An error occurred: {str(e)}"

def main():
    """Handles interaction with C# via stdin/stdout."""
    print("Python model ready")  # Ensure C# receives this signal
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
            response = generate_response()

            # Print response for C# to read
            print(response)
            sys.stdout.flush()

        except Exception as e:
            print(f"ERROR: {str(e)}", file=sys.stderr)
            sys.stderr.flush()

if __name__ == "__main__":
    print("Starting Python script...")

    if sys.stdin.isatty():  # Running in CLI mode
        print("Running in interactive mode. Type 'exit' to quit.")
        while True:
            input_text = input("You: ")
            if input_text.lower() == "exit":
                print("Exiting interactive mode.")
                break
            
            response = generate_response()
            print("Model:", response)

    else:  # Running inside C# service
        print("Running in main mode (C# integration).")
        sys.stdout.flush()
        main()
