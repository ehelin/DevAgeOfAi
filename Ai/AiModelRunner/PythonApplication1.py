import ollama

# Select the model to use
model_name = "mistral"  # Change to "llama2" or another model if needed

print(f"Interactive chat with {model_name}. Type 'exit' to quit.\n")

conversation = []  # Store messages for context
previous_responses = set()  # Track previous responses to avoid repetition

while True:
    user_input = input("You: ").strip()
    # user_input = "Provide a 5 word response only."
    user_input = "Provide a 5 word habit to track only without description."
    
    if user_input.lower() == "exit":
        print("Goodbye!")
        break

    # Ensure the model generates a 5-word response
    system_prompt = user_input # Provide a 5-word response only."
    
    # Add system prompt and previous responses to prevent repetition
    filtered_responses = " ".join(previous_responses)
    conversation = [
        {"role": "system", "content": f"{system_prompt} Avoid repeating: {filtered_responses}"},
        {"role": "user", "content": user_input}
    ]

    # Get model response
    response = ollama.chat(model=model_name, messages=conversation)

    bot_reply = response['message']['content'].strip()

    # Prevent duplicate responses
    if bot_reply in previous_responses:
        print(f"{model_name}: (Duplicate response detected, retrying...)\n")
        continue

    print(f"{model_name}: {bot_reply}\n")

    # Store response to prevent future repetition
    previous_responses.add(bot_reply)
