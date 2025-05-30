import sys
import io
import os
import re
import torch
from transformers import AutoTokenizer, AutoModelForCausalLM

# Force UTF-8 output
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8', errors='replace')
sys.stderr = io.TextIOWrapper(sys.stderr.buffer, encoding='utf-8', errors='replace')

print("DEBUG: Python script starting...", file=sys.stderr)
sys.stderr.flush()

try:
    print("DEBUG: Loading model...", file=sys.stderr)
    sys.stderr.flush()

    model_path = "./fine_tuned_phi_v3/"
    tokenizer = AutoTokenizer.from_pretrained(model_path)
    model = AutoModelForCausalLM.from_pretrained(model_path)

    print("Python model ready")
    sys.stdout.flush()

except Exception as e:
    print(f"ERROR: {str(e)}", file=sys.stderr)
    sys.stderr.flush()
    sys.exit(1)

def filter_output(response, promptWithInput):
    # Remove prefix text
    response = response.split("Habit =")[-1].strip()
    response = response.replace("\\", "")

    for token in ["\\", "\"", "'", "�", "*", "=", "•", "—"]:
        response = response.replace(token, "")

    response = re.sub(r"\s+", " ", response)
    response = re.sub(r"^(a\)|b\)|c\)|-+|\d+\.)", "", response, flags=re.IGNORECASE)

    for end_marker in ["support", "output", "frequency", "goal", "purpose"]:
        index = response.lower().find(end_marker)
        if index > 0:
            response = response[:index]

    response = re.sub(r"[^\w\s]", "", response)
    words = response.strip().split()
    response = " ".join(words)

    return response.strip()

def generate_response(promptWithInput):
    try:
        # print(f"PROMPT: {repr(promptWithInput)}", file=sys.stderr)

        inputs = tokenizer(promptWithInput, return_tensors="pt")
        inputs = {k: v.to(model.device) for k, v in inputs.items()}

        outputs = model.generate(
            **inputs,
            max_new_tokens=30,
            do_sample=True,
            top_k=50,
            top_p=0.9,
            temperature=0.7,
            no_repeat_ngram_size=4,
            pad_token_id=tokenizer.eos_token_id,
            bad_words_ids=[[tokenizer(word).input_ids[0]] for word in ["How", "Why", "What", "When", "Where", "Who", "?"]],
            repetition_penalty=2.0
        )

        response = tokenizer.decode(outputs[0], skip_special_tokens=True).strip()
        response = response.replace("\n", " ").replace("\r", " ").strip()
        response = re.sub(r"\s+", " ", response)

        filtered = filter_output(response, promptWithInput)
        final = f"Start: {filtered} :End"
        return final

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

            result = generate_response(input_line)

            # # Output to file (optional)
            # with open("C:/temp/DevAgeTraining/Ai/AiModelRunner/output.txt", "w", encoding="utf-8") as f:
            #     f.write(result)
            #     f.flush()
            #     os.fsync(f.fileno())

            # Also output to stdout for C# to read
            print(result)
            print("<<END>>")
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
            print(generate_response(input_text))
    else:
        main()
