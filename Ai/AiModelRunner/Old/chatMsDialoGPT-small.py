from transformers import AutoModelForCausalLM, AutoTokenizer
import torch

tokenizer = AutoTokenizer.from_pretrained("microsoft/DialoGPT-small", padding_size="left")
model = AutoModelForCausalLM.from_pretrained("microsoft/DialoGPT-small")

while True:
    inputText = input(">> User:")
    
    if inputText.lower() == 'exit':
        break

    inputText = inputText + tokenizer.eos_token
    new_user_input_ids = tokenizer.encode(inputText, return_tensors='pt')

    # append the new user input tokens to the chat history
    # bot_input_ids = torch.cat([chat_history_ids, new_user_input_ids], dim=-1) 

    # generated a response while limiting the total chat history to 1000 tokens, 
    chat_history_ids = model.generate(new_user_input_ids, max_length=1000, pad_token_id=tokenizer.eos_token_id)

    # pretty print last ouput tokens from bot
    result1 = chat_history_ids[:, new_user_input_ids.shape[-1]:][0]
    result2 = tokenizer.decode(result1, skip_special_tokens=True)
                             
    print("DialoGPT: {}".format(result2))
