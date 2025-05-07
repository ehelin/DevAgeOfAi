from transformers import AutoTokenizer, AutoModelForCausalLM, Trainer, TrainingArguments
from datasets import load_dataset
import torch
import time
import shutil
import os

# Path to save the fine-tuned model
output_dir = "./phi3_finetuned"

# Delete existing model directory if it exists
if os.path.exists(output_dir):
    print(f"🗑️ Deleting existing fine-tuned model: {output_dir}")
    shutil.rmtree(output_dir)

# Load model & tokenizer
model_name = "microsoft/Phi-3.5-mini-instruct"
tokenizer = AutoTokenizer.from_pretrained(model_name)
model = AutoModelForCausalLM.from_pretrained(model_name, torch_dtype=torch.float16).to("cuda" if torch.cuda.is_available() else "cpu")

# Load dataset
dataset_path = "habits_train.jsonl"  # Ensure this file exists in the same directory
print(f"📂 Loading dataset from: {dataset_path}")
dataset = load_dataset("json", data_files={"train": dataset_path})

# Tokenization function
def tokenize_function(example):
    text = f"User: {example['input']} AI: {example['output']}"
    tokenized = tokenizer(text, padding="max_length", truncation=True, max_length=128)
    tokenized["labels"] = tokenized["input_ids"]  # Labels should match inputs for causal LM fine-tuning
    return tokenized

# Tokenize dataset
tokenized_dataset = dataset.map(tokenize_function, batched=True)

# Fine-tuning settings
training_args = TrainingArguments(
    output_dir=output_dir,  # Directory to save model
    per_device_train_batch_size=4,  # Adjust based on GPU VRAM
    num_train_epochs=3,  # Adjust epochs based on dataset size
    save_strategy="epoch",  # Save after each epoch
    save_total_limit=5,  # Keep last 5 checkpoints
    logging_dir="./logs",  # Directory for logs
    logging_steps=50,  # Log training progress
    learning_rate=2e-5,  # Lower LR for better fine-tuning
    weight_decay=0.01,
    fp16=torch.cuda.is_available(),  # Use mixed precision if supported
    gradient_accumulation_steps=8,  # Helps smaller GPUs handle training
    optim="adamw_torch",
    report_to="none",  # Disable logging to external services
)

# Trainer
trainer = Trainer(
    model=model,
    args=training_args,
    train_dataset=tokenized_dataset["train"],
    tokenizer=tokenizer,
)

# Start training and track time
start_time = time.time()
print("🚀 Starting fine-tuning...")

trainer.train()

# Save the fine-tuned model
trainer.save_model(output_dir)
tokenizer.save_pretrained(output_dir)

# Print total training time
end_time = time.time()
elapsed_time = (end_time - start_time) / 60
print(f"✅ Fine-tuning complete! Model saved to '{output_dir}' (Training took {elapsed_time:.2f} minutes).")