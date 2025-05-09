import torch
from transformers import AutoModelForCausalLM, AutoTokenizer, TrainingArguments
from peft import LoraConfig, get_peft_model
from datasets import load_dataset
from trl import SFTTrainer

# 1. Load Base Phi Model
model_name = "microsoft/Phi-3.5-mini-instruct"
tokenizer = AutoTokenizer.from_pretrained(model_name)
model = AutoModelForCausalLM.from_pretrained(
    model_name,
    device_map="auto"
)

# 2. Configure LoRA
lora_config = LoraConfig(
    r=16,
    lora_alpha=32,
    lora_dropout=0.05,
    target_modules=["linear1", "linear2", "lm_head"]  # Compatible with Phi-3.5
)

# 3. Apply LoRA adapters
model = get_peft_model(model, lora_config)

# 4. Load dataset
dataset = load_dataset("json", data_files="training_data.json")["train"]

# 5. Tokenize dataset
def tokenize_function(example):
    return tokenizer(example["input"], truncation=True)

tokenized_dataset = dataset.map(tokenize_function, batched=True)

# 6. Training arguments
training_args = TrainingArguments(
    output_dir="./qlora-output",
    per_device_train_batch_size=2,
    gradient_accumulation_steps=4,
    num_train_epochs=3,
    logging_steps=10,
    save_steps=100,
    save_total_limit=1,
    fp16=True,  # Mixed precision
    optim="adamw_torch"
)

# 7. Trainer setup
trainer = SFTTrainer(
    model=model,
    train_dataset=tokenized_dataset,
    args=training_args,
    peft_config=lora_config
)

# 8. Start training
trainer.train()

# 9. Save model
model.save_pretrained("./fine_tuned_phi")
tokenizer.save_pretrained("./fine_tuned_phi")
