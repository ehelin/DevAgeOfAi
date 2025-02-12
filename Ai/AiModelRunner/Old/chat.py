from transformers import AutoTokenizer, AutoModelForCausalLM
import sys
import json

# Load the tokenizer and model from Hugging Face
model_name = "microsoft/Phi-3.5-mini-instruct"
tokenizer = AutoTokenizer.from_pretrained(model_name)
model = AutoModelForCausalLM.from_pretrained(model_name)

def generate_response(input_text):
    # Encode the input text
    inputs = tokenizer(input_text, return_tensors="pt")

    # Generate a response
    outputs = model.generate(**inputs, max_length=100)

    # Decode and return the response
    response = tokenizer.decode(outputs[0], skip_special_tokens=True)
    return response

if __name__ == "__main__":
    print("Python model ready")  # Signal to C# that Python is ready
    sys.stdout.flush()

    while True:
        # Read input from standard input
        input_line = sys.stdin.readline().strip()
        
        # Exit condition
        if input_line == "exit":
            print("Python exiting")
            sys.stdout.flush()
            break
        
        # Generate response
        response = generate_response(input_line)
        
        # Output the response as JSON to handle special characters
        print(json.dumps({"response": response}))
        sys.stdout.flush()
