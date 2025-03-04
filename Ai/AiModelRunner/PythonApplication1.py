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

        # Updated system instruction to enforce a single response
        system_instruction = (
            "Respond with only ONE habit in exactly 5 words. "
            "Return nothing else. Do not list multiple habits. "
            "Example: 'Read One Chapter Every Day'. "
            "Ensure responses are unique and actionable."
        )

        # Build the conversation context with prior responses
        filtered_responses = " ".join(previous_responses)
        conversation = [
            {"role": "system", "content": f"{system_instruction} Avoid repeating: {filtered_responses}"},
            {"role": "user", "content": prompt}
        ]

        # Get model response
        response_data = ollama.chat(model=model_name, messages=conversation)
        response = response_data['message']['content'].strip()

        # # Clean up response
        # response = response.replace("\"", " ").strip()
        # response = response.replace("\n", " ").strip()
        # response = response.replace(prompt, "").strip()

        # Extract only the first valid 5-word response
        response_lines = response.split("\n")
        for line in response_lines:
            response_words = line.split()
            if len(response_words) == 5:
                response = " ".join(response_words)
                break

        # Ensure response is unique
        if response and response not in previous_responses:
            previous_responses.add(response)
            break

    return response if response else "No valid response found."

def interactive_mode(model_name):
    """Handles interaction via standard input from the C# wrapper or command prompt."""
    print("Python model ready")
    sys.stdout.flush()  # Ensure the output is sent immediately

    while True:
        try:
            # Only show "You: " prompt if running in a terminal
            if sys.stdin.isatty():
                print("You: ", end="", flush=True)

            input_text = sys.stdin.readline().strip()  # Read input
            
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
    interactive_mode(model_name)  # Start interactive mode
