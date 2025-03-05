import ollama
import sys
import re
from difflib import SequenceMatcher

# Default model if none is specified
DEFAULT_MODEL = "mistral:7b"

# Store previous responses to avoid repetition
previous_responses = set()

def normalize_text(text):
    """Normalize text to prevent duplicates with minor differences."""
    text = text.lower().strip()
    text = re.sub(r'[^\w\s]', '', text)  # Remove punctuation
    text = re.sub(r'\bsteps?\b', 'step', text)  # Normalize variations of "step"
    text = re.sub(r'\bminutes?\b', 'minute', text)  # Normalize time units
    text = re.sub(r'\bhours?\b', 'hour', text)
    text = re.sub(r'\bdaily\b', 'day', text)  # Standardize "daily" to "day"
    text = re.sub(r'\bweekly\b', 'week', text)
    text = re.sub(r'\btrack\b', 'log', text)  # Reduce verb variations
    text = re.sub(r'\bmonitor\b', 'log', text)
    text = re.sub(r'\bjournalize\b', 'journal', text)  # Avoid unnecessary variations
    text = re.sub(r'\bmeditate\b', 'meditation', text)  # Catch meditation phrasing
    text = re.sub(r'\bgratitudes?\b', 'gratitude', text)  # Singular form normalization
    text = re.sub(r'\s+', ' ', text)  # Remove extra spaces
    return text

def is_similar(a, b, threshold=0.8):
    """Check if two phrases are similar based on a threshold."""
    return SequenceMatcher(None, a, b).ratio() > threshold

def is_unique_response(response):
    """Check if a response is unique compared to previous ones."""
    normalized_response = normalize_text(response)

    for prev in previous_responses:
        if is_similar(normalized_response, prev):
            return False  # Too similar, reject

    return True

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
            "Ensure responses are unique in meaning, not just wording. "
            "Do not rephrase an existing response. "
            "Example: 'Read One Chapter Every Day'. "
            f"Avoid repeating any habits related to: {', '.join(previous_responses)}"
        )

        # Build the conversation context with prior responses
        conversation = [
            {"role": "system", "content": system_instruction},
            {"role": "user", "content": prompt}
        ]

        # Get model response
        response_data = ollama.chat(model=model_name, messages=conversation)
        response = response_data['message']['content'].strip()

        # Normalize response to check for duplicates
        normalized_response = normalize_text(response)

        # Ensure response is unique
        if normalized_response and is_unique_response(normalized_response):
            previous_responses.add(normalized_response)  # Store normalized version
            break

    return response if response else "No valid response found."

def interactive_mode(model_name):
    """Handles interaction via standard input from the C# wrapper or command prompt."""
    print("Python model ready")
    sys.stdout.flush()  # Ensure the output is sent immediately

    while True:
        try:
            if sys.stdin.isatty():  # Show prompt only if in command line
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
