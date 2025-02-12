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

    # Updated prompt
    promptWithInput = "Please suggest a habit that can be tracked."

    while attempts < max_attempts:
        attempts += 1

        # Tokenize input and remove last token to prevent repetition
        inputs = tokenizer(
            promptWithInput,
            return_tensors="pt",
            padding=True,
            truncation=True,
            add_special_tokens=True
        )

        input_ids = inputs["input_ids"]
        attention_mask = inputs["attention_mask"]

        # Prevent repeating prompt by removing last token
        input_ids = input_ids[:, :-1]

        outputs = model.generate(
            input_ids,
            attention_mask=attention_mask,
            max_new_tokens=5,  # Force exactly 5 words
            do_sample=True,
            temperature=0.8,
            top_k=50,
            top_p=0.9,
            repetition_penalty=1.5,  # Penalize repetition more strongly
            pad_token_id=tokenizer.eos_token_id,
            num_return_sequences=1
        )

        # Get response (should already be exactly 5 words)
        response = tokenizer.decode(outputs[0], skip_special_tokens=True).strip()

        # Ensure line breaks are removed and text is cleaned up
        response = response.replace("\n", " ").strip()
        
        # Ensure the prompt is removed from the response
        response = response.replace(promptWithInput, "").strip()

        # Debugging: Show the full generated response before filtering
        # sys.stderr.write(f"DEBUG: Model response (exactly 5 words): {response}\n")
        # sys.stderr.flush()

        # Ensure it doesn't echo the prompt
        if response and not response.startswith("Please suggest a habit"):
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
       
