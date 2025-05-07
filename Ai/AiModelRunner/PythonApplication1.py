from transformers import AutoTokenizer, AutoModelForCausalLM
import sys
import json
import torch

# Load the tokenizer and model from Hugging Face
model_name = "microsoft/Phi-3.5-mini-instruct"
# model_name = "./phi3_finetuned_model"
# model_name = "./phi3_finetuned_long"
tokenizer = AutoTokenizer.from_pretrained(model_name)
model = AutoModelForCausalLM.from_pretrained(model_name)

# Dictionary to store prompts and their responses
seen_responses = {}

def filter_response(response, src_string, add_length):    
    """Removes or trims response based on a given source string."""
    pos = response.find(src_string)
    if pos != -1:
        if add_length:
            response = response[pos + len(src_string):]
        else:    
            response = response[:pos]
    return response

def generate_response(input_line, max_attempts=3):
    """Generates a response using Phi-3.5-mini while ensuring uniqueness."""
    try:
        promptWithInput = input_line #"Please give a response of less than 5 words"

        # Check if this prompt has been processed before
        if promptWithInput in seen_responses:
            previous_responses = seen_responses[promptWithInput]
        else:
            previous_responses = set()

        response = None
        attempts = 0

        while attempts < max_attempts:
            attempts += 1

            # Encode the input text
            inputs = tokenizer(promptWithInput, return_tensors="pt")

            # Ensure model is running on the correct device (CPU/GPU)
            inputs = {k: v.to(model.device) for k, v in inputs.items()}

            # Generate a response with adjusted parameters
            # outputs = model.generate(
            #     **inputs,
            #     max_length=30,  # Reduce long, unfocused outputs
            #     temperature=0.5,  # Makes responses more controlled
            #     top_p=0.85,  # Prevents overly random responses
            #     num_return_sequences=1,
            #     pad_token_id=tokenizer.eos_token_id
            # )
            outputs = model.generate(
                **inputs,
                max_new_tokens=30,
                num_beams=5,  # Still ensures structure
                do_sample=True,  # Enable sampling to avoid repetition
                temperature=0.7,  # Slight randomness (adjust as needed)
                top_p=0.9  # Nucleus sampling
            )


            # Decode the response
            if outputs.size(0) > 0:
                response = tokenizer.decode(outputs[0], skip_special_tokens=True)

                # Clean up the response
                # response = filter_response(response, "Habit Name:[", True)
                # response = filter_response(response, "Habit Name:", True)
                response = response.replace(input_line, "")
                # response = filter_response(response, "\n", False)
                # response = response.replace(":", "")
                # response = response.replace("\n", "")

                # Ensure response is unique
                if response not in previous_responses:
                    break  # Unique response found

        if response is None or response in previous_responses:
            response = "No new unique response could be generated."

        # Store the response for this prompt
        previous_responses.add(response)
        seen_responses[promptWithInput] = previous_responses

        return response

    except Exception as e:
        return f"An error occurred: {str(e)}"

def main():
    """Handles interaction with C# via stdin/stdout."""
    print("Python model ready")  # Signal to C# that Python is ready
    sys.stdout.flush()

    while True:
        try:
            # Read input from standard input
            input_line = sys.stdin.readline().strip()

            # Exit condition
            if input_line.lower() == "exit":
                print("Python exiting")
                sys.stdout.flush()
                break

            # Generate response
            response = generate_response(input_line)

            # Debugging information (optional)
            sys.stderr.write(f"DEBUG: Output generated: {response}\n")
            sys.stderr.flush()

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
        main()