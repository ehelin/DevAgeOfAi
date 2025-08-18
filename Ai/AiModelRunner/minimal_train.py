#!/usr/bin/env python
"""
Minimal training script - most compatible version.
Uses basic PyTorch training loop to avoid library conflicts.
"""

import os
import torch
import warnings
from transformers import AutoModelForCausalLM, AutoTokenizer
from peft import LoraConfig, get_peft_model, TaskType
from torch.utils.data import Dataset, DataLoader
import random

warnings.filterwarnings("default", category=UserWarning, module="transformers")

MODEL_NAME = "microsoft/Phi-3.5-mini-instruct"
OUTPUT_DIR = "./fine_tuned_phi_habits"
CACHE_DIR = "./model_cache"

class HabitDataset(Dataset):
    """Simple dataset for habit training"""
    def __init__(self, tokenizer, num_examples=100):
        self.tokenizer = tokenizer
        self.examples = []
        
        habits = [
            "Walk ten thousand steps every day",
            "Exercise thirty minutes each morning daily",
            "Drink eight glasses of water daily",
            "Meditate ten minutes every single morning",
            "Read twenty pages of book daily",
        ]
        
        prompt = "Please suggest a habit that can be tracked"
        
        for _ in range(num_examples // len(habits)):
            for habit in habits:
                messages = [
                    {"role": "user", "content": prompt},
                    {"role": "assistant", "content": habit}
                ]
                text = tokenizer.apply_chat_template(messages, tokenize=False, add_generation_prompt=False)
                
                # Tokenize immediately
                tokens = tokenizer(text, truncation=True, max_length=128, padding="max_length", return_tensors="pt")
                self.examples.append({
                    "input_ids": tokens["input_ids"].squeeze(),
                    "attention_mask": tokens["attention_mask"].squeeze()
                })
        
        random.shuffle(self.examples)
    
    def __len__(self):
        return len(self.examples)
    
    def __getitem__(self, idx):
        item = self.examples[idx]
        return {
            "input_ids": item["input_ids"],
            "attention_mask": item["attention_mask"],
            "labels": item["input_ids"].clone()
        }

def train_step(model, batch, optimizer):
    """Single training step"""
    model.train()
    optimizer.zero_grad()
    
    outputs = model(
        input_ids=batch["input_ids"],
        attention_mask=batch["attention_mask"],
        labels=batch["labels"]
    )
    
    loss = outputs.loss
    loss.backward()
    optimizer.step()
    
    return loss.item()

def main():
    print("\n" + "=" * 50)
    print("MINIMAL HABIT MODEL TRAINING")
    print("=" * 50)
    
    if os.path.exists(OUTPUT_DIR):
        response = input(f"\n{OUTPUT_DIR} exists. Delete? (y/n): ")
        if response.lower() == 'y':
            import shutil
            shutil.rmtree(OUTPUT_DIR)
        else:
            print("Exiting.")
            return
    
    # Load tokenizer
    print("\nLoading tokenizer...")
    tokenizer = AutoTokenizer.from_pretrained(MODEL_NAME, cache_dir=CACHE_DIR, trust_remote_code=True)
    if tokenizer.pad_token is None:
        tokenizer.pad_token = tokenizer.eos_token
    
    # Load model
    print("Loading model (this may take a few minutes)...")
    model = AutoModelForCausalLM.from_pretrained(
        MODEL_NAME,
        cache_dir=CACHE_DIR,
        torch_dtype=torch.float32,
        device_map="cpu",  # Force CPU to avoid device issues
        trust_remote_code=True,
        low_cpu_mem_usage=True
    )
    
    # Apply LoRA
    print("\nApplying LoRA...")
    lora_config = LoraConfig(
        task_type=TaskType.CAUSAL_LM,
        r=4,  # Smaller rank for faster training
        lora_alpha=8,
        lora_dropout=0.1,
        target_modules=["qkv_proj", "o_proj"],
        bias="none"
    )
    
    model = get_peft_model(model, lora_config)
    model.print_trainable_parameters()
    
    # Create dataset and dataloader
    print("\nPreparing data...")
    dataset = HabitDataset(tokenizer, num_examples=50)  # Small dataset
    dataloader = DataLoader(dataset, batch_size=1, shuffle=True)
    
    # Optimizer
    optimizer = torch.optim.AdamW(model.parameters(), lr=1e-4)
    
    # Training loop
    print("\nTraining...")
    print("This will take about 5-10 minutes on CPU...")
    
    model.train()
    num_epochs = 2  # Just 2 epochs for quick training
    
    for epoch in range(num_epochs):
        total_loss = 0
        print(f"\nEpoch {epoch + 1}/{num_epochs}")
        
        for i, batch in enumerate(dataloader):
            loss = train_step(model, batch, optimizer)
            total_loss += loss
            
            if (i + 1) % 10 == 0:
                avg_loss = total_loss / (i + 1)
                print(f"  Step {i + 1}/{len(dataloader)} - Loss: {avg_loss:.4f}")
        
        print(f"Epoch {epoch + 1} complete - Avg Loss: {total_loss / len(dataloader):.4f}")
    
    # Save model
    print("\nSaving model...")
    model.save_pretrained(OUTPUT_DIR)
    tokenizer.save_pretrained(OUTPUT_DIR)
    
    print(f"\n✅ Training complete! Model saved to {OUTPUT_DIR}")
    
    # Quick test
    print("\nTesting model...")
    model.eval()
    test_prompt = "Please suggest a habit that can be tracked"
    messages = [{"role": "user", "content": test_prompt}]
    text = tokenizer.apply_chat_template(messages, tokenize=False, add_generation_prompt=True)
    inputs = tokenizer(text, return_tensors="pt", truncation=True)
    
    with torch.no_grad():
        outputs = model.generate(
            input_ids=inputs["input_ids"],
            attention_mask=inputs["attention_mask"],
            max_new_tokens=10,
            temperature=0.5,
            do_sample=True,
            pad_token_id=tokenizer.pad_token_id
        )
    
    response = tokenizer.decode(outputs[0][len(inputs["input_ids"][0]):], skip_special_tokens=True)
    print(f"Sample output: {response}")

if __name__ == "__main__":
    main()