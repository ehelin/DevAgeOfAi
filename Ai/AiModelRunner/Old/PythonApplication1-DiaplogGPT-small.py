from transformers import AutoTokenizer, AutoModelForCausalLM
import sys
import torch

# Load the tokenizer and model from Hugging Face
model_name = "microsoft/DialoGPT-small"
tokenizer = AutoTokenizer.from_pretrained(model_name)
model = AutoModelForCausalLM.from_pretrained(model_name)

# Set pad token explicitly (DialoGPT doesn't have a pad token)
tokenizer.pad_token = tokenizer.eos_token

def generate_response(max_attempts=3):
    attempts = 0
    response = None

    # Improved prompt with a strong example to guide the model
    promptWithInput = (
        "User: What is a good daily habit to track?\n"
        "Assistant: A good daily habit to track is exercising for 30 minutes. Another example is"
    )

    while attempts < max_attempts:
        attempts += 1

        # Tokenize input
        inputs = tokenizer(
            promptWithInput,
            return_tensors="pt",
            padding=True,
            truncation=True,
            add_special_tokens=True
        )

        input_ids = inputs["input_ids"]
        attention_mask = inputs["attention_mask"]

        outputs = model.generate(
            input_ids,
            attention_mask=attention_mask,
            max_new_tokens=200,  # Ensures detailed responses
            min_length=10,  # Ensures responses are complete
            do_sample=True,  # Enables randomness for variety
            temperature=0.9,  # Higher temp for more creative responses
            num_beams=3,  # Keeps responses structured
            repetition_penalty=2.0,  # Prevents generic responses
            length_penalty=1.5,  # Encourages longer, more structured responses
            pad_token_id=tokenizer.eos_token_id,
            num_return_sequences=1
        )

        # Decode response, skipping prompt
        response = tokenizer.decode(outputs[0], skip_special_tokens=True).strip()

        # Ensure line breaks are removed and text is cleaned up
        response = response.replace("\n", " ").strip()

        # Ensure the prompt is removed from the response
        response = response.replace(promptWithInput, "").strip()

        # Ensure response is meaningful and not an echo or incomplete
        if response and not response.lower().startswith("user:") and len(response.split()) >= 5:
            break  # Valid response found

    return response if response else "No valid response found."

def main():
    print("Python model ready")  # Signal to C# that Python is ready
    sys.stdout.flush()

    while True:
        # Read input from standard input
        input_line = sys.stdin.readline().strip()
        
        # Exit condition
        if input_line.lower() == "exit":
            print("Python exiting")
            sys.stdout.flush()
            break
        
        # Generate response
        response = generate_response()

        # Debugging information (optional)
        # sys.stderr.write(f"DEBUG: Final output: {response}\n")
        # sys.stderr.flush()
        
        print(response)
        sys.stdout.flush()

if __name__ == "__main__":
    print("Starting python script...")

    if sys.stdin.isatty():
        print("Running in interactive mode. Type 'exit' to quit.")
        while True:
            input_text = input("You: ")
            if input_text.lower() == "exit":
                print("Exiting interactive mode.")
                break
            
            response = generate_response()
            print("Model:", response)
    else:
        print("Running in main mode.")
        main()
