# from transformers import AutoModelForCausalLM, AutoTokenizer
# import torch
# Load model directly
from transformers import AutoTokenizer, AutoModelForCausalLM

# STart w/this one - error is Tokenizer class Qwen2Tokenizer does not exist or is not currently imported.
tokenizer = AutoTokenizer.from_pretrained("M4-ai/tau-0.5B")
model = AutoModelForCausalLM.from_pretrained("M4-ai/tau-0.5B")

# general chat - Jagged conversations with unexpected pivots and non-sensical answers
# tokenizer = AutoTokenizer.from_pretrained("microsoft/DialoGPT-small", padding_size="left")
# model = AutoModelForCausalLM.from_pretrained("microsoft/DialoGPT-small")

# general chat - Better conversations, but no depth.  Mostly parroted responses and pivots on what you say
# tokenizer = AutoTokenizer.from_pretrained("microsoft/DialoGPT-large", padding_size="left")
# model = AutoModelForCausalLM.from_pretrained("microsoft/DialoGPT-large")

# # Massive paragraph generator (ignored input request)
# tokenizer = AutoTokenizer.from_pretrained("openai-community/gpt2", padding_size="left")
# model = AutoModelForCausalLM.from_pretrained("openai-community/gpt2")

# # Error running - amu/spin-phi2 does not appear to have a file named configuration_phi.py. Checkout 'https://huggingface.co/amu/spin-phi2/main' for available files.
# tokenizer = AutoTokenizer.from_pretrained("amu/spin-phi2", trust_remote_code=True)
# model = AutoModelForCausalLM.from_pretrained("amu/spin-phi2", trust_remote_code=True)

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
