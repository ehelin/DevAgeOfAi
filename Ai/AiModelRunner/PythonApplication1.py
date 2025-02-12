import ollama
import sys

# Default model if none is specified
DEFAULT_MODEL = "mistral:7b"

# Store previous responses to avoid repetition
previous_responses = set()

def generate_response(model_name, prompt, max_attempts=3):
    """Generate a response using the specified model while avoiding repetition."""
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
        # response_words = response.split()
        # if len(response_words) >= 5:
        #     response = " ".join(response_words[:5])  # Truncate to exactly 5 words

        # Ensure response is unique
        if response and response not in previous_responses:
            previous_responses.add(response)
            break

    return response if response else "No valid response found."

def interactive_mode(model_name):
    """Handles interaction via standard input from the C# wrapper."""
    print("Python model ready")
    sys.stdout.flush()  # Ensure the output is sent immediately

    while True:
        try:
            input_text = sys.stdin.readline().strip()  # Read from C# process
            if not input_text:
                continue  # Ignore empty input
            
            if input_text.lower() == "exit":
                print("Exiting interactive mode.")
                sys.stdout.flush()
                break

            response = generate_response(model_name, input_text)
            print(response)
            sys.stdout.flush()  # Ensure C# receives output immediately

        except Exception as e:
            print(f"Error: {str(e)}")
            sys.stdout.flush()

if __name__ == "__main__":
    model_name = DEFAULT_MODEL  # Default model

    # Start interactive mode to continuously accept input
    interactive_mode(model_name)
