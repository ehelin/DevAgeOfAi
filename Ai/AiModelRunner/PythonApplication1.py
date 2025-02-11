import ollama
import sys

# Select the model to use
model_name = "mistral:7b"

# Store previous responses to avoid repetition
previous_responses = set()

def generate_response(prompt, max_attempts=3):
    """Generate a response using the Mistral model while avoiding repetition."""
    if not prompt:
        return "Error: Prompt cannot be empty."

    attempts = 0
    response = None

    while attempts < max_attempts:
        attempts += 1

        # Build the conversation context with prior responses
        filtered_responses = " ".join(previous_responses)
        conversation = [
            {"role": "system", "content": f"{prompt} Avoid repeating: {filtered_responses}"},
            {"role": "user", "content": prompt}
        ]

        # Get model response
        response_data = ollama.chat(model=model_name, messages=conversation)
        response = response_data['message']['content'].strip()

        # Clean up response
        response = response.replace("\n", " ").strip()
        response = response.replace(prompt, "").strip()

        # Ensure response is exactly 5 words (if applicable)
        response_words = response.split()
        if len(response_words) >= 5:
            response = " ".join(response_words[:5])  # Truncate to exactly 5 words

        # Ensure response is unique
        if response and response not in previous_responses:
            previous_responses.add(response)
            break

    return response if response else "No valid response found."

def main():
    """Handles interaction via standard input for integration with PythonScriptService.cs."""
    print("Python model ready")
    sys.stdout.flush()

    while True:
        input_line = sys.stdin.readline().strip()
        
        if input_line.lower() == "exit":
            print("Python exiting")
            sys.stdout.flush()
            break

        response = generate_response(input_line)
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
            
            response = generate_response(input_text)
            print("Model:", response)
    else:
        print("Running in main mode.")
        main()
