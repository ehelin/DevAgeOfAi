#!/usr/bin/env python
"""
Simplified training script using standard Trainer instead of SFTTrainer.
More compatible across different library versions.
"""

import os
import sys
import torch
import warnings
from transformers import (
    AutoModelForCausalLM,
    AutoTokenizer,
    Trainer,
    TrainingArguments,
    DataCollatorForLanguageModeling
)
from datasets import Dataset
from peft import LoraConfig, get_peft_model, TaskType
import random

# SECURITY: Enable all security warnings
warnings.filterwarnings("default", category=UserWarning, module="transformers")

# Configuration
MODEL_NAME = "microsoft/Phi-3.5-mini-instruct"
OUTPUT_DIR = "./fine_tuned_phi_habits"
CACHE_DIR = "./model_cache"

def prepare_dataset(tokenizer):
    """Prepare training data"""
    print("\nPreparing training data...")
    
    # Habit templates
    habits = [
        "Walk ten thousand steps every day",
        "Exercise thirty minutes each morning daily",
        "Drink eight glasses of water daily",
        "Meditate ten minutes every single morning",
        "Read twenty pages of book daily",
        "Write three gratitudes every night daily",
        "Sleep full eight hours every night",
        "Practice piano thirty minutes every day",
        "Take vitamins with breakfast every morning",
        "Journal fifteen minutes before bed nightly",
    ]
    
    # Create training examples
    examples = []
    prompt = "Please suggest a habit that can be tracked"
    
    for _ in range(20):  # 20 copies of each for 200 total examples
        for habit in habits:
            messages = [
                {"role": "user", "content": prompt},
                {"role": "assistant", "content": habit}
            ]
            
            text = tokenizer.apply_chat_template(
                messages,
                tokenize=False,
                add_generation_prompt=False
            )
            examples.append({"text": text})
    
    random.shuffle(examples)
    
    # Tokenize dataset
    def tokenize_function(examples):
        # Process each text individually to avoid nested list issues
        tokenized = tokenizer(
            examples["text"],
            truncation=True,
            padding="max_length",
            max_length=256
        )
        # Copy input_ids to labels
        tokenized["labels"] = tokenized["input_ids"].copy()
        return tokenized
    
    # Create and process dataset
    dataset = Dataset.from_list(examples)
    tokenized_dataset = dataset.map(
        tokenize_function, 
        batched=True,
        remove_columns=["text"]  # Remove the text column after tokenization
    )
    
    # Set format for PyTorch
    tokenized_dataset.set_format("torch", columns=["input_ids", "attention_mask", "labels"])
    
    # Split into train/eval
    split = tokenized_dataset.train_test_split(test_size=0.1, seed=42)
    
    print(f"Training examples: {len(split['train'])}")
    print(f"Evaluation examples: {len(split['test'])}")
    
    return split

def main():
    print("\n" + "=" * 50)
    print("SIMPLIFIED HABIT MODEL TRAINING")
    print("=" * 50)
    
    # Check if output exists
    if os.path.exists(OUTPUT_DIR):
        response = input(f"\n{OUTPUT_DIR} already exists. Delete and retrain? (y/n): ")
        if response.lower() != 'y':
            print("Exiting.")
            return
        import shutil
        shutil.rmtree(OUTPUT_DIR)
    
    # Load tokenizer
    print("\nLoading tokenizer...")
    tokenizer = AutoTokenizer.from_pretrained(
        MODEL_NAME,
        cache_dir=CACHE_DIR,
        trust_remote_code=True,
        code_revision=None
    )
    
    if tokenizer.pad_token is None:
        tokenizer.pad_token = tokenizer.eos_token
        tokenizer.pad_token_id = tokenizer.eos_token_id
    
    # Load model
    print("Loading model...")
    print("This may take a while if downloading for the first time...")
    
    model = AutoModelForCausalLM.from_pretrained(
        MODEL_NAME,
        cache_dir=CACHE_DIR,
        torch_dtype=torch.float32 if not torch.cuda.is_available() else torch.float16,
        device_map="auto",
        trust_remote_code=True,
        code_revision=None,
        low_cpu_mem_usage=True
    )
    
    # Apply LoRA
    print("\nApplying LoRA configuration...")
    lora_config = LoraConfig(
        task_type=TaskType.CAUSAL_LM,
        r=8,
        lora_alpha=16,
        lora_dropout=0.1,
        target_modules=["qkv_proj", "o_proj"],
        bias="none"
    )
    
    model = get_peft_model(model, lora_config)
    model.print_trainable_parameters()
    
    # Prepare dataset
    dataset = prepare_dataset(tokenizer)
    
    # Training arguments
    training_args = TrainingArguments(
        output_dir=OUTPUT_DIR,
        num_train_epochs=3,
        per_device_train_batch_size=1,  # Small batch size for CPU
        per_device_eval_batch_size=1,
        gradient_accumulation_steps=8,
        warmup_steps=100,
        logging_steps=10,
        save_strategy="epoch",
        eval_strategy="epoch",
        save_total_limit=2,
        load_best_model_at_end=True,
        metric_for_best_model="loss",
        greater_is_better=False,
        report_to="none",
        optim="adamw_torch",
        learning_rate=5e-5,
        fp16=torch.cuda.is_available(),
        push_to_hub=False,
        remove_unused_columns=False,
    )
    
    # Data collator - simplified version
    data_collator = DataCollatorForLanguageModeling(
        tokenizer=tokenizer,
        mlm=False,  # Causal LM, not masked LM
        pad_to_multiple_of=None  # Remove padding to multiple to avoid issues
    )
    
    # Create trainer
    trainer = Trainer(
        model=model,
        args=training_args,
        train_dataset=dataset["train"],
        eval_dataset=dataset["test"],
        data_collator=data_collator,
    )
    
    # Train
    print("\nStarting training...")
    print("This will take 10-20 minutes on CPU...")
    trainer.train()
    
    # Save model
    print("\nSaving model...")
    trainer.save_model()
    tokenizer.save_pretrained(OUTPUT_DIR)
    
    print(f"\n✅ Training complete! Model saved to {OUTPUT_DIR}")
    
    # Test the model
    print("\nTesting the fine-tuned model...")
    test_prompt = "Please suggest a habit that can be tracked"
    
    messages = [{"role": "user", "content": test_prompt}]
    formatted = tokenizer.apply_chat_template(messages, tokenize=False, add_generation_prompt=True)
    inputs = tokenizer(formatted, return_tensors="pt", truncation=True)
    
    with torch.no_grad():
        outputs = model.generate(
            **inputs,
            max_new_tokens=10,
            temperature=0.5,
            do_sample=True,
            pad_token_id=tokenizer.pad_token_id
        )
    
    response = tokenizer.decode(outputs[0][len(inputs['input_ids'][0]):], skip_special_tokens=True)
    print(f"Sample output: {response}")

if __name__ == "__main__":
    main()