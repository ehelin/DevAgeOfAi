from transformers import AutoTokenizer, AutoModelForCausalLM
import sys
import torch

# Load the tokenizer and model from Hugging Face
model_name = "gpt2-large"
tokenizer = AutoTokenizer.from_pretrained(model_name)
model = AutoModelForCausalLM.from_pretrained(model_name)

# Set pad token explicitly (GPT-2 does not have a pad token)
tokenizer.pad_token = tokenizer.eos_token

# Define bad words to strictly block questions
bad_words = ["How", "Why", "What", "When", "Where", "Who", "?", "explain"]
bad_words_ids = [tokenizer.encode(word, add_special_tokens=False) for word in bad_words]

# Define forced words to ensure response is a habit action
force_words = ["Track", "Write", "Drink", "Exercise", "Read", "Meditate", "Sleep"]
force_words_ids = [tokenizer.encode(word, add_special_tokens=False) for word in force_words]

def contains_question(response):
    """Check if the response contains a question"""
    question_words = ["how", "why", "what", "when", "where", "who", "?"]
    return any(word in response.lower() for word in question_words)

def generate_response(max_attempts=3):
    attempts = 0
    response = None

    promptWithInput = "Provide a 5 word daily habit to track:"

    while attempts < max_attempts:
        attempts += 1

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
            max_new_tokens=30,  # Allows long responses but filtered later
            do_sample=False,  # Structured response without randomness
            num_beams=5,  # Ensures structured, logical responses
            repetition_penalty=3.5,  # Prevents repetition
            length_penalty=1.0,  # Allows balanced length
            no_repeat_ngram_size=2,  # Eliminates repeating phrases
            pad_token_id=tokenizer.eos_token_id,
            num_return_sequences=1,
            bad_words_ids=bad_words_ids,  # Blocks all question-related words
            force_words_ids=force_words_ids  # Forces response to be a habit
        )

        response = tokenizer.decode(outputs[0], skip_special_tokens=True).strip()
        response = response.replace("\n", " ").strip()
        response = response.replace(promptWithInput, "").strip()

        # Ensure response is exactly 5 words and not a question
        response_words = response.split()
        if len(response_words) >= 5:
            response = " ".join(response_words[:5])  # Truncate to exactly 5 words
        
        if response and not contains_question(response):
            break  # Ensure response does not contain a question

    return response if response else "No valid response found."

def main():
    print("Python model ready")
    sys.stdout.flush()

    while True:
        input_line = sys.stdin.readline().strip()
        
        if input_line.lower() == "exit":
            print("Python exiting")
            sys.stdout.flush()
            break
        
        response = generate_response()
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
