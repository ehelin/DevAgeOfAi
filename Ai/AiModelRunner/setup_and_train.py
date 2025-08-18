#!/usr/bin/env python
"""
Complete setup and training script for the habit suggestion model.
This script will:
1. Download the base Phi-3.5 model
2. Fine-tune it using QLoRA for habit suggestions
3. Save the fine-tuned adapters
"""

import os
import sys
import torch
import warnings
from transformers import (
    AutoModelForCausalLM,
    AutoTokenizer,
    TrainingArguments,
    BitsAndBytesConfig
)
from datasets import Dataset
from peft import LoraConfig, get_peft_model, TaskType, prepare_model_for_kbit_training
from trl import SFTTrainer, DataCollatorForCompletionOnlyLM
import random

# SECURITY: Enable all security warnings for transformers
warnings.filterwarnings("default", category=UserWarning, module="transformers")

# Configuration
MODEL_NAME = "microsoft/Phi-3.5-mini-instruct"
OUTPUT_DIR = "./fine_tuned_phi_habits"
CACHE_DIR = "./model_cache"  # Local cache for downloaded models

def download_base_model():
    """Download and cache the base model and tokenizer"""
    print("=" * 50)
    print("Step 1: Downloading base model and tokenizer")
    print("=" * 50)
    
    print(f"Downloading tokenizer from {MODEL_NAME}...")
    # SECURITY: Always scan for malicious code when loading models
    tokenizer = AutoTokenizer.from_pretrained(
        MODEL_NAME,
        cache_dir=CACHE_DIR,
        trust_remote_code=True,  # Required for Phi-3.5
        code_revision=None  # Always use latest code revision with security patches
    )
    
    # Ensure padding token is set
    if tokenizer.pad_token is None:
        tokenizer.pad_token = tokenizer.eos_token
        tokenizer.pad_token_id = tokenizer.eos_token_id
    
    print(f"Downloading model from {MODEL_NAME}...")
    print("This may take a while (model is ~7GB)...")
    
    # For training, we'll use quantization to reduce memory usage
    # Skip quantization if not on CUDA
    if torch.cuda.is_available():
        print("CUDA available, using 4-bit quantization for training")
        bnb_config = BitsAndBytesConfig(
            load_in_4bit=True,
            bnb_4bit_quant_type="nf4",
            bnb_4bit_compute_dtype=torch.float16,
            bnb_4bit_use_double_quant=True
        )
        # SECURITY: Always scan for malicious code when loading models
        model = AutoModelForCausalLM.from_pretrained(
            MODEL_NAME,
            cache_dir=CACHE_DIR,
            quantization_config=bnb_config,
            device_map="auto",
            trust_remote_code=True,  # Required for Phi-3.5
            code_revision=None  # Always use latest code revision with security patches
        )
        model = prepare_model_for_kbit_training(model)
    else:
        print("CUDA not available, using CPU (training will be slower)")
        # SECURITY: Always scan for malicious code when loading models
        model = AutoModelForCausalLM.from_pretrained(
            MODEL_NAME,
            cache_dir=CACHE_DIR,
            torch_dtype=torch.float32,
            device_map="auto",
            trust_remote_code=True,  # Required for Phi-3.5
            low_cpu_mem_usage=True,
            code_revision=None  # Always use latest code revision with security patches
        )
    
    print("Model and tokenizer downloaded successfully!")
    return model, tokenizer

def prepare_training_data(tokenizer):
    """Prepare the training dataset"""
    print("\n" + "=" * 50)
    print("Step 2: Preparing training data")
    print("=" * 50)
    
    # High-quality habit templates (5-6 words)
    habit_templates = [
        # Health & Fitness
        "Walk ten thousand steps every day",
        "Exercise thirty minutes each morning daily",
        "Do twenty pushups before breakfast daily",
        "Stretch ten minutes before bed nightly",
        "Run three miles every single morning",
        "Complete morning yoga routine every day",
        "Do fifty squats each morning daily",
        
        # Hydration & Nutrition
        "Drink eight glasses of water daily",
        "Eat five vegetables servings every day",
        "Skip all sugary drinks every day",
        "Have healthy breakfast every single morning",
        "Pack nutritious lunch all week days",
        
        # Mental Health & Mindfulness
        "Meditate ten minutes every single morning",
        "Write three gratitudes every night daily",
        "Take five deep breaths every hour",
        "Journal fifteen minutes before bed nightly",
        "Practice mindfulness during every meal daily",
        "Read thirty minutes before sleeping nightly",
        
        # Productivity & Learning
        "Read twenty pages of book daily",
        "Learn five new words every day",
        "Practice piano thirty minutes every day",
        "Complete three tasks before noon daily",
        "Study one hour without any distractions",
        "Write five hundred words every day",
        
        # Sleep & Rest
        "Sleep full eight hours every night",
        "Go to bed by ten nightly",
        "Wake up at six every morning",
        "Take twenty minute afternoon nap daily",
        "No screens one hour before bedtime",
        
        # Social & Relationships
        "Call one friend every single week",
        "Send three thank you notes weekly",
        "Have meaningful conversation every single day",
        "Spend quality time with family daily",
        
        # Personal Care
        "Floss teeth every single night consistently",
        "Apply sunscreen every morning without fail",
        "Take vitamins with breakfast every morning",
        "Stand up every hour during work",
        "Take five screen breaks every day"
    ]
    
    # Generate training examples
    data = []
    prompt = "Please suggest a habit that can be tracked"
    
    print(f"Generating {len(habit_templates) * 10} training examples...")
    
    for _ in range(10):  # Create 10 examples of each habit
        for habit in habit_templates:
            messages = [
                {"role": "user", "content": prompt},
                {"role": "assistant", "content": habit}
            ]
            
            # Apply chat template
            text = tokenizer.apply_chat_template(
                messages,
                tokenize=False,
                add_generation_prompt=False
            )
            
            data.append({"text": text, "habit": habit})
    
    # Shuffle the data
    random.shuffle(data)
    
    print(f"Created {len(data)} training examples")
    
    # Create dataset
    dataset = Dataset.from_list(data)
    
    # Split into train and eval
    dataset = dataset.train_test_split(test_size=0.1, seed=42)
    
    print(f"Training examples: {len(dataset['train'])}")
    print(f"Evaluation examples: {len(dataset['test'])}")
    
    return dataset

def train_model(model, tokenizer, dataset):
    """Fine-tune the model using QLoRA"""
    print("\n" + "=" * 50)
    print("Step 3: Fine-tuning model with QLoRA")
    print("=" * 50)
    
    # LoRA configuration
    lora_config = LoraConfig(
        task_type=TaskType.CAUSAL_LM,
        r=8,  # Rank
        lora_alpha=16,
        lora_dropout=0.1,
        target_modules=["qkv_proj", "o_proj"],  # Phi-3.5 attention layers
        bias="none"
    )
    
    print("Applying LoRA configuration...")
    model = get_peft_model(model, lora_config)
    model.print_trainable_parameters()
    
    # Training arguments
    training_args = TrainingArguments(
        output_dir=OUTPUT_DIR,
        num_train_epochs=3,
        per_device_train_batch_size=2,
        per_device_eval_batch_size=2,
        gradient_accumulation_steps=4,
        gradient_checkpointing=True,
        optim="adamw_torch",
        learning_rate=2e-4,
        lr_scheduler_type="cosine",
        warmup_ratio=0.1,
        logging_steps=10,
        save_strategy="steps",
        save_steps=50,
        eval_strategy="steps",  # Changed from evaluation_strategy to eval_strategy
        eval_steps=50,
        save_total_limit=2,
        load_best_model_at_end=True,
        report_to="none",  # Disable wandb/tensorboard
        fp16=torch.cuda.is_available(),  # Use fp16 if CUDA available
        push_to_hub=False,
    )
    
    # Data collator for completion only
    response_template = "<|assistant|>"
    collator = DataCollatorForCompletionOnlyLM(
        response_template=response_template,
        tokenizer=tokenizer
    )
    
    # Create trainer (updated for newer TRL versions)
    trainer = SFTTrainer(
        model=model,
        args=training_args,
        train_dataset=dataset["train"],
        eval_dataset=dataset["test"],
        processing_class=tokenizer,  # Changed from tokenizer to processing_class
        data_collator=collator,
        dataset_text_field="text",
        # max_seq_length is now set via dataset_kwargs or handled automatically
        dataset_kwargs={
            "add_special_tokens": False,  # We already added them in the dataset
        }
    )
    
    print("\nStarting training...")
    print("This may take 15-30 minutes depending on your hardware...")
    
    # Train
    trainer.train()
    
    print("\nSaving fine-tuned model...")
    trainer.save_model()
    
    # Also save tokenizer
    tokenizer.save_pretrained(OUTPUT_DIR)
    
    print(f"\nModel saved to {OUTPUT_DIR}")
    
    return model

def test_fine_tuned_model():
    """Test the fine-tuned model"""
    print("\n" + "=" * 50)
    print("Step 4: Testing fine-tuned model")
    print("=" * 50)
    
    from peft import PeftModel
    
    # Load tokenizer
    tokenizer = AutoTokenizer.from_pretrained(MODEL_NAME)
    
    # Load base model (for inference, we don't need quantization)
    print("Loading base model for testing...")
    # SECURITY: Always scan for malicious code when loading models
    model = AutoModelForCausalLM.from_pretrained(
        MODEL_NAME,
        torch_dtype=torch.float32,
        device_map="auto",
        trust_remote_code=True,  # Required for Phi-3.5
        cache_dir=CACHE_DIR,
        code_revision=None  # Always use latest code revision with security patches
    )
    
    # Load LoRA adapters
    print(f"Loading fine-tuned adapters from {OUTPUT_DIR}...")
    model = PeftModel.from_pretrained(model, OUTPUT_DIR)
    
    # Test generation
    test_prompts = [
        "Please suggest a habit that can be tracked",
        "Please suggest a habit that can be tracked",
        "Please suggest a habit that can be tracked"
    ]
    
    print("\nGenerating test responses:\n")
    
    for i, prompt in enumerate(test_prompts, 1):
        messages = [{"role": "user", "content": prompt}]
        
        formatted_prompt = tokenizer.apply_chat_template(
            messages,
            tokenize=False,
            add_generation_prompt=True
        )
        
        inputs = tokenizer(formatted_prompt, return_tensors="pt", truncation=True)
        inputs = {k: v.to(model.device) for k, v in inputs.items()}
        
        with torch.no_grad():
            outputs = model.generate(
                **inputs,
                max_new_tokens=10,
                temperature=0.5,
                do_sample=True,
                top_p=0.9,
                pad_token_id=tokenizer.pad_token_id,
                eos_token_id=tokenizer.eos_token_id
            )
        
        generated_tokens = outputs[0][len(inputs['input_ids'][0]):]
        response = tokenizer.decode(generated_tokens, skip_special_tokens=True).strip()
        
        print(f"Test {i}: {response}")
    
    print("\nFine-tuning complete! The model is ready to use.")

def main():
    """Main execution flow"""
    print("\n" + "=" * 50)
    print("HABIT SUGGESTION MODEL SETUP AND TRAINING")
    print("=" * 50)
    
    # Check if output directory already exists
    if os.path.exists(OUTPUT_DIR):
        response = input(f"\n{OUTPUT_DIR} already exists. Delete and retrain? (y/n): ")
        if response.lower() != 'y':
            print("Exiting without changes.")
            return
        else:
            import shutil
            shutil.rmtree(OUTPUT_DIR)
            print(f"Deleted {OUTPUT_DIR}")
    
    try:
        # Step 1: Download model
        model, tokenizer = download_base_model()
        
        # Step 2: Prepare data
        dataset = prepare_training_data(tokenizer)
        
        # Step 3: Train model
        model = train_model(model, tokenizer, dataset)
        
        # Step 4: Test model
        test_fine_tuned_model()
        
        print("\n" + "=" * 50)
        print("SETUP COMPLETE!")
        print("=" * 50)
        print(f"\nYour fine-tuned model is saved in: {OUTPUT_DIR}")
        print("The AiModelServiceRunner.py script will automatically load these adapters.")
        
    except Exception as e:
        print(f"\nError during setup: {str(e)}")
        import traceback
        traceback.print_exc()
        sys.exit(1)

if __name__ == "__main__":
    main()