from transformers import AutoTokenizer, AutoModelForCausalLM
from peft import PeftModel
import torch
import sys

def test_base_model():
    """Test the base Phi-3.5 model with proper prompting"""
    print("Testing base Phi-3.5 model with improved prompting...")
    
    model_name = "microsoft/Phi-3.5-mini-instruct"
    tokenizer = AutoTokenizer.from_pretrained(model_name)
    model = AutoModelForCausalLM.from_pretrained(
        model_name,
        torch_dtype=torch.bfloat16,
        device_map="auto"
    )
    
    # Test with proper chat template
    test_prompts = [
        "Please suggest a habit that can be tracked",
        "Give me a specific daily habit I can monitor and track. Be concise and specific.",
        "Suggest one trackable health habit. Reply with just the habit, no explanation."
    ]
    
    for prompt in test_prompts:
        # Use Phi-3.5's chat template
        messages = [
            {"role": "system", "content": "You are a habit tracking assistant. When asked for habit suggestions, provide specific, actionable, trackable habits. Be concise."},
            {"role": "user", "content": prompt}
        ]
        
        # Apply the chat template
        text = tokenizer.apply_chat_template(messages, tokenize=False, add_generation_prompt=True)
        inputs = tokenizer(text, return_tensors="pt").to(model.device)
        
        # Generate with controlled parameters
        outputs = model.generate(
            **inputs,
            max_new_tokens=30,
            temperature=0.7,
            do_sample=True,
            top_p=0.9,
            pad_token_id=tokenizer.pad_token_id,
            eos_token_id=tokenizer.eos_token_id,
        )
        
        response = tokenizer.decode(outputs[0], skip_special_tokens=True)
        
        # Extract assistant response
        if "<|assistant|>" in response:
            response = response.split("<|assistant|>")[-1].strip()
        
        print(f"\nPrompt: {prompt}")
        print(f"Response: {response}")
        print("-" * 50)

def test_fine_tuned_model(model_path="./fine_tuned_phi_habits"):
    """Test the fine-tuned model"""
    print(f"\nTesting fine-tuned model from {model_path}...")
    
    try:
        # Load base model
        base_model = AutoModelForCausalLM.from_pretrained(
            "microsoft/Phi-3.5-mini-instruct",
            torch_dtype=torch.bfloat16,
            device_map="auto"
        )
        
        # Load LoRA weights
        model = PeftModel.from_pretrained(base_model, model_path)
        tokenizer = AutoTokenizer.from_pretrained(model_path)
        
        test_prompts = [
            "Please suggest a habit that can be tracked",
            "What's a good daily habit to monitor?",
            "Suggest a health-related habit"
        ]
        
        model.eval()
        for prompt in test_prompts:
            messages = [{"role": "user", "content": prompt}]
            
            text = tokenizer.apply_chat_template(messages, tokenize=False, add_generation_prompt=True)
            inputs = tokenizer(text, return_tensors="pt").to(model.device)
            
            with torch.no_grad():
                outputs = model.generate(
                    **inputs,
                    max_new_tokens=30,
                    temperature=0.7,
                    do_sample=True,
                    top_p=0.9,
                    pad_token_id=tokenizer.pad_token_id,
                    eos_token_id=tokenizer.eos_token_id,
                )
            
            response = tokenizer.decode(outputs[0], skip_special_tokens=True)
            if "<|assistant|>" in response:
                response = response.split("<|assistant|>")[-1].strip()
            
            print(f"\nPrompt: {prompt}")
            print(f"Response: {response}")
            print("-" * 50)
            
    except Exception as e:
        print(f"Could not load fine-tuned model: {e}")
        print("Run improved_qlora_train.py first to create the fine-tuned model.")

def interactive_test():
    """Interactive testing mode"""
    print("\n" + "="*60)
    print("INTERACTIVE HABIT MODEL TEST")
    print("="*60)
    
    # Test base model with proper prompting
    model_name = "microsoft/Phi-3.5-mini-instruct"
    tokenizer = AutoTokenizer.from_pretrained(model_name)
    model = AutoModelForCausalLM.from_pretrained(
        model_name,
        torch_dtype=torch.bfloat16,
        device_map="auto"
    )
    
    print("\nUsing base Phi-3.5 model with optimized prompting")
    print("Type 'exit' to quit\n")
    
    while True:
        user_input = input("You: ").strip()
        if user_input.lower() == 'exit':
            break
        
        # Use system prompt for better results
        messages = [
            {"role": "system", "content": "You are a habit tracking assistant. Suggest specific, measurable, trackable daily habits. Examples: 'Walk 10000 steps daily', 'Drink 8 glasses of water', 'Read for 30 minutes before bed'. Be concise and specific."},
            {"role": "user", "content": user_input}
        ]
        
        text = tokenizer.apply_chat_template(messages, tokenize=False, add_generation_prompt=True)
        inputs = tokenizer(text, return_tensors="pt").to(model.device)
        
        outputs = model.generate(
            **inputs,
            max_new_tokens=30,
            temperature=0.7,
            do_sample=True,
            top_p=0.9,
            pad_token_id=tokenizer.pad_token_id,
            eos_token_id=tokenizer.eos_token_id,
        )
        
        response = tokenizer.decode(outputs[0], skip_special_tokens=True)
        if "<|assistant|>" in response:
            response = response.split("<|assistant|>")[-1].strip()
        
        print(f"Model: {response}\n")

if __name__ == "__main__":
    if len(sys.argv) > 1 and sys.argv[1] == "--interactive":
        interactive_test()
    else:
        print("="*60)
        print("HABIT MODEL TESTING SUITE")
        print("="*60)
        
        # Test base model with improved prompting
        test_base_model()
        
        # Test fine-tuned model if available
        test_fine_tuned_model()
        
        print("\n" + "="*60)
        print("To run interactive mode: python test_habit_model.py --interactive")
        print("="*60)