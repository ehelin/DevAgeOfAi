from transformers import AutoTokenizer, AutoModelForCausalLM

# Load the tokenizer and model from Hugging Face
model_name = "microsoft/Phi-3.5-mini-instruct"
tokenizer = AutoTokenizer.from_pretrained(model_name)
model = AutoModelForCausalLM.from_pretrained(model_name)

print("Chat with the model! Type 'exit' to end the conversation.\n")

while True:
    # Get user input
    input_text = input("You: ")
    
    # Exit condition
    if input_text.lower() == 'exit':
        print("Exiting the chat. Goodbye!")
        break

    # Encode the input text
    inputs = tokenizer(input_text, return_tensors="pt")
    
    # Generate a response
    outputs = model.generate(**inputs, max_length=100)
    
    # Decode and print the response
    response = tokenizer.decode(outputs[0], skip_special_tokens=True)
    print(f"Model: {response}\n")
