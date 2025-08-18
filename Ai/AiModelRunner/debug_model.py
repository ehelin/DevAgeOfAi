#!/usr/bin/env python
"""Debug script to test the fine-tuned model"""

from transformers import AutoTokenizer, AutoModelForCausalLM
from peft import PeftModel
import torch
import os
import warnings

# SECURITY: Enable all security warnings for transformers
warnings.filterwarnings("default", category=UserWarning, module="transformers")

# Model configuration
model_name = "microsoft/Phi-3.5-mini-instruct"
adapter_path = "./fine_tuned_phi_habits"

print("Loading tokenizer...")
tokenizer = AutoTokenizer.from_pretrained(model_name)

print("Loading base model...")
# SECURITY: Always scan for malicious code when loading models
model = AutoModelForCausalLM.from_pretrained(
    model_name,
    torch_dtype=torch.float32,  # Use float32 for better precision
    device_map="auto",
    trust_remote_code=True,  # Required for Phi-3.5
    code_revision=None  # Always use latest code revision with security patches
)

# Check if adapters exist and load them
if os.path.exists(adapter_path):
    print(f"Loading fine-tuned adapters from {adapter_path}...")
    model = PeftModel.from_pretrained(model, adapter_path)
    print("Fine-tuned adapters loaded successfully!")
    print(f"Model type: {type(model)}")
else:
    print(f"No adapters found at {adapter_path}")

def test_generation():
    prompt = "Please suggest a habit that can be tracked"
    
    # Simple format matching training
    messages = [
        {"role": "user", "content": prompt}
    ]
    
    # Apply chat template
    formatted_prompt = tokenizer.apply_chat_template(
        messages,
        tokenize=False,
        add_generation_prompt=True
    )
    
    print(f"\nFormatted prompt:\n{formatted_prompt}\n")
    
    # Tokenize
    inputs = tokenizer(formatted_prompt, return_tensors="pt", truncation=True, max_length=512)
    inputs = {k: v.to(model.device) for k, v in inputs.items()}
    
    print("Generating responses with different parameters...\n")
    
    # Test different generation parameters
    param_sets = [
        {"max_new_tokens": 10, "temperature": 0.1, "do_sample": False},  # Greedy
        {"max_new_tokens": 10, "temperature": 0.5, "do_sample": True, "top_p": 0.9},  # Moderate sampling
        {"max_new_tokens": 8, "temperature": 0.7, "do_sample": True, "top_p": 0.95},  # Higher temperature
    ]
    
    for i, params in enumerate(param_sets, 1):
        print(f"Test {i} - Parameters: {params}")
        
        with torch.no_grad():
            outputs = model.generate(
                **inputs,
                pad_token_id=tokenizer.pad_token_id,
                eos_token_id=tokenizer.eos_token_id,
                **params
            )
        
        # Decode full response
        full_response = tokenizer.decode(outputs[0], skip_special_tokens=True)
        
        # Extract generated part only
        generated_tokens = outputs[0][len(inputs['input_ids'][0]):]
        generated_text = tokenizer.decode(generated_tokens, skip_special_tokens=True).strip()
        
        print(f"Generated text: {generated_text}")
        print(f"Word count: {len(generated_text.split())}")
        print()

if __name__ == "__main__":
    test_generation()