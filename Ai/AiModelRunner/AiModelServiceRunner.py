import sys
import io
import torch
from transformers import AutoTokenizer, AutoModelForCausalLM

# Ensure UTF-8 output (especially for C# or console)
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8', errors='replace')
sys.stderr = io.TextIOWrapper(sys.stderr.buffer, encoding='utf-8', errors='replace')

print("DEBUG: Python script starting...", file=sys.stderr)
sys.stderr.flush()

try:
    print("DEBUG: Loading fine-tuned model...", file=sys.stderr)
    sys.stderr.flush()

    # Load fine-tuned model and tokenizer from local directory
    model_path = "./fine_tuned_phi"
    tokenizer = AutoTokenizer.from_pretrained(model_path)
    model = AutoModelForCausalLM.from_pretrained(model_path)

    print("Python model ready")
    sys.stdout.flush()

except Exception as e:
    print(f"ERROR: Failed to load model: {str(e)}", file=sys.stderr)
    sys.stderr.flush()
    sys.exit(1)

def filter_output(response, promptWithInput):
    response = response[len(promptWithInput):].strip()
    for ch in ["[", ",", "-", "Γ"]:
        index = response.find(ch)
        if index != -1:
            response = response[:index]
    return " ".join(response.splitlines()).strip()

def generate_response():
    try:
        promptWithInput = "Provide a habit to track only without description.\nHabit:"
        inputs = tokenizer(promptWithInput, return_tensors="pt")
        inputs = {k: v.to(model.device) for k, v in inputs.items()}

        outputs = model.generate(
            **inputs,
            max_new_tokens=30,
            num_beams=5,
            do_sample=False,
            temperature=0.3,
            pad_token_id=tokenizer.eos_token_id,
            force_words_ids=[[tokenizer(word).input_ids[0]] for word in ["Track", "Write", "Drink", "Exercise", "Read", "Meditate", "Sleep"]],
            bad_words_ids=[[tokenizer(word).input_ids[0]] for word in ["How", "Why", "What", "When", "Where", "Who", "?"]],
            repetition_penalty=1.5
        )

        response = tokenizer.decode(outputs[0], skip_special_tokens=True).strip()
        return filter_output(response, promptWithInput)

    except Exception as e:
        print(f"ERROR: Failed to generate response: {str(e)}", file=sys.stderr)
        sys.stderr.flush()
        return f"An error occurred: {str(e)}"

def main():
    print("Python model ready")
    sys.stdout.flush()
    while True:
        try:
            input_line = sys.stdin.readline().strip()
            if not input_line:
                continue
            if input_line.lower() == "exit":
                print("Python exiting")
                sys.stdout.flush()
                break
            print(generate_response())
            sys.stdout.flush()
        except Exception as e:
            print(f"ERROR: {str(e)}", file=sys.stderr)
            sys.stderr.flush()

if __name__ == "__main__":
    if sys.stdin.isatty():
        print("Running in interactive mode. Type 'exit' to quit.")
        while True:
            input_text = input("You: ")
            if input_text.lower() == "exit":
                print("Exiting interactive mode.")
                break
            print("Model:", generate_response())
    else:
        main()
