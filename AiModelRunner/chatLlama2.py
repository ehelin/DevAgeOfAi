import subprocess

#========================================================
# Load model directly
from transformers import AutoTokenizer, AutoModelForCausalLM

tokenizer = AutoTokenizer.from_pretrained("meta-llama/Llama-2-7b-chat-hf")
model = AutoModelForCausalLM.from_pretrained("meta-llama/Llama-2-7b-chat-hf")
#========================================================

def interact_with_command_prompt():
    while True:
        command_input = input("Enter your command (type 'exit' to quit): ")
        
        if command_input.lower() == 'exit':
            break
                    
        inputs = tokenizer(command_input, return_tensors="pt") 
        
        generate_ids = model.generate(inputs.input_ids, max_length=1000)
                
        result = tokenizer.batch_decode(generate_ids, skip_special_tokens=True)[0]
        answerSections = result.split(' ')
                
        print("Llama2: {}".format(answerSections[1]))        

# Call the function to start interacting with the command prompt
interact_with_command_prompt()