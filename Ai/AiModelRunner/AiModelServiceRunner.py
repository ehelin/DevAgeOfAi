import sys
import io
import os
from phi_model_core import generate_response

# Force UTF-8 output
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8', errors='replace')
sys.stderr = io.TextIOWrapper(sys.stderr.buffer, encoding='utf-8', errors='replace')

print("DEBUG: Python script starting...", file=sys.stderr)
sys.stderr.flush()

print("Python model ready")
sys.stdout.flush()

def main():
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
