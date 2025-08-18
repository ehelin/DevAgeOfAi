from transformers import (
    AutoModelForCausalLM,
    AutoTokenizer,
    TrainingArguments,
    Trainer,
    DataCollatorForLanguageModeling
)
from datasets import Dataset
from peft import LoraConfig, get_peft_model, TaskType
import torch
import json
import random
import warnings

# Suppress specific warnings related to cache compatibility
warnings.filterwarnings("ignore", message=".*DynamicCache.*")
warnings.filterwarnings("ignore", message=".*get_max_length.*")

# Model configuration
MODEL_NAME = "microsoft/Phi-3.5-mini-instruct"
MODEL_REVISION = "main"
OUTPUT_DIR = "./fine_tuned_phi_habits"

def create_training_data():
    """Generate high-quality 5-6 word habit training data"""
    
    # Exact 5-6 word habits
    habits = [
        "Walk ten thousand steps every day",
        "Drink eight glasses water every day", 
        "Exercise thirty minutes each morning daily",
        "Read twenty pages before bed nightly",
        "Meditate ten minutes every morning daily",
        "Write three gratitudes every night",
        "Do fifty pushups each morning",
        "Stretch ten minutes before bedtime",
        "Take vitamins with breakfast daily",
        "Floss teeth every single night",
        "Call one friend every week",
        "Sleep eight hours every night",
        "Eat five vegetables servings daily",
        "Practice piano thirty minutes daily",
        "Journal fifteen minutes before bed",
        "Stand up every hour today",
        "Take five deep breaths hourly",
        "Apply sunscreen every morning routine",
        "Pack healthy lunch every workday",
        "Learn five new words daily"
    ]
    
    # Create training examples
    data = []
    prompt = "Please suggest a habit that can be tracked"
    
    # Generate multiple examples (each habit used multiple times)
    for _ in range(100):  # 100 examples total
        habit = random.choice(habits)
        
        # Create simple prompt-response format
        text = f"User: {prompt}\nAssistant: {habit}<|endoftext|>"
        data.append({"text": text})
    
    return data

def tokenize_function(examples, tokenizer):
    """Tokenize the text data"""
    # Tokenize with padding and truncation
    tokenized = tokenizer(
        examples["text"],
        truncation=True,
        padding="max_length",
        max_length=128,  # Short sequences for habits
        return_tensors="pt"
    )
    
    # For causal LM, labels are the same as input_ids
    tokenized["labels"] = tokenized["input_ids"].clone()
    
    return tokenized

def main():
    print("Starting simple training...")
    
    # Load tokenizer
    print("Loading tokenizer...")
    tokenizer = AutoTokenizer.from_pretrained(
        MODEL_NAME,
        revision=MODEL_REVISION,
        trust_remote_code=True
    )
    
    # Set padding token
    if tokenizer.pad_token is None:
        tokenizer.pad_token = tokenizer.eos_token
    
    # Load model with comprehensive compatibility fixes
    print("Loading model...")
    model = AutoModelForCausalLM.from_pretrained(
        MODEL_NAME,
        revision=MODEL_REVISION,
        trust_remote_code=True,
        torch_dtype=torch.float32,  # Use float32 for stability
        device_map="auto",
        attn_implementation="eager",  # Fix DynamicCache compatibility issue
        use_cache=False,  # Disable caching to avoid DynamicCache issues
        low_cpu_mem_usage=True  # Additional memory optimization
    )
    
    # Configure LoRA with correct Phi-3.5 module names
    print("Setting up LoRA...")
    peft_config = LoraConfig(
        task_type=TaskType.CAUSAL_LM,
        r=8,  # Smaller rank for stability
        lora_alpha=16,
        lora_dropout=0.1,
        target_modules=["qkv_proj", "o_proj"],  # Correct Phi-3.5 attention modules
        bias="none",
    )
    
    # Apply LoRA
    model = get_peft_model(model, peft_config)
    model.print_trainable_parameters()
    
    # Create training data
    print("Creating training data...")
    train_data = create_training_data()
    
    # Save sample for inspection
    with open("training_sample.json", "w") as f:
        json.dump(train_data[:3], f, indent=2)
    
    print(f"Created {len(train_data)} training examples")
    
    # Create dataset
    dataset = Dataset.from_list(train_data)
    
    # Tokenize dataset
    print("Tokenizing data...")
    tokenized_dataset = dataset.map(
        lambda x: tokenize_function(x, tokenizer),
        batched=True,
        remove_columns=dataset.column_names
    )
    
    # Data collator
    data_collator = DataCollatorForLanguageModeling(
        tokenizer=tokenizer,
        mlm=False,  # Causal LM, not masked LM
        pad_to_multiple_of=8
    )
    
    # Training arguments - very conservative with cache disabled
    training_args = TrainingArguments(
        output_dir=OUTPUT_DIR,
        num_train_epochs=2,  # Just 2 epochs for testing
        per_device_train_batch_size=1,
        gradient_accumulation_steps=2,
        learning_rate=5e-5,  # Lower learning rate
        warmup_steps=10,
        logging_steps=5,
        save_strategy="epoch",
        evaluation_strategy="no",  # No evaluation
        optim="adamw_torch",
        fp16=False,
        bf16=False,
        dataloader_pin_memory=False,
        remove_unused_columns=True,
        report_to=["none"],
        use_cache=False,  # Disable cache in training args too
    )
    
    # Create trainer
    trainer = Trainer(
        model=model,
        args=training_args,
        train_dataset=tokenized_dataset,
        data_collator=data_collator,
        tokenizer=tokenizer,
    )
    
    # Train
    print("Starting training...")
    trainer.train()
    
    # Save model
    print("Saving model...")
    trainer.save_model()
    tokenizer.save_pretrained(OUTPUT_DIR)
    
    print(f"Training complete! Model saved to {OUTPUT_DIR}")
    
    # Test the model
    print("\nTesting the model...")
    model.eval()
    
    test_prompt = "User: Please suggest a habit that can be tracked\nAssistant:"
    inputs = tokenizer(test_prompt, return_tensors="pt").to(model.device)
    
    with torch.no_grad():
        outputs = model.generate(
            **inputs,
            max_new_tokens=15,
            temperature=0.7,
            do_sample=True,
            top_p=0.9,
            pad_token_id=tokenizer.pad_token_id,
            eos_token_id=tokenizer.eos_token_id,
        )
    
    response = tokenizer.decode(outputs[0], skip_special_tokens=True)
    response = response[len(test_prompt):].strip()
    
    print(f"Test response: {response}")

if __name__ == "__main__":
    main()