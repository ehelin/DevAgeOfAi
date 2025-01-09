from transformers import AutoTokenizer, AutoModelForCausalLM
import sys
import json

# Load the tokenizer and model from Hugging Face
model_name = "microsoft/Phi-3.5-mini-instruct"
tokenizer = AutoTokenizer.from_pretrained(model_name)
model = AutoModelForCausalLM.from_pretrained(model_name)

# Dictionary to store prompts and their responses
seen_responses = {}

def filter_response(response, src_string, add_length):    
    pos = response.find(src_string)
    if pos != -1:
        if add_length:
            response = response[pos + len(src_string):]
        else:    
            response = response[:pos]

    return response

def generate_response(input_line, max_attempts=3):
    try:
        # prompt = "Please respond to the following question independently:"
        # promptWithInput = "Suggest a habit to track for improving mental health. Respond strictly in this format and do not repeat this instruction or provide an example:\nHabit Name: [Name of the habit]\nBrief Description: [Brief explanation of how it benefits mental health]"
        promptWithInput = "Suggest a habit to track for improving health. Provide the response only in this exact format:\nHabit Name:[Name of the habit]\nDo not repeat the prompt or provide any additional context."

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
            
            # Generate a response with adjusted parameters
            outputs = model.generate(
                **inputs,
                max_length=100,
                do_sample=True,  # Enable sampling for varied responses
                temperature=0.9,  # Add randomness
                top_p=0.9,  # Enable nucleus sampling
                num_return_sequences=1,
                pad_token_id=tokenizer.eos_token_id
            )

            # Decode the response
            if outputs.size(0) > 0:
                response = tokenizer.decode(outputs[0], skip_special_tokens=True)

                # Filter and clean up the response
                response = filter_response(response, "Habit Name:[", True)
                response = filter_response(response, "Habit Name:", True)
                response = filter_response(response, "\n", False)      
                # response = response.replace(prompt, "")
                response = response.replace(input_line, "")
                response = response.replace(":", "")
                response = response.replace("\n", "")

                # Check if the response is unique
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

        # Debugging information (optional)
        sys.stderr.write(f"DEBUG: Output generated: {response}\n")
        sys.stderr.flush()
        
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
