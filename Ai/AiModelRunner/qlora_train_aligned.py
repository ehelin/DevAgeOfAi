
import torch
from transformers import AutoModelForCausalLM, AutoTokenizer, TrainingArguments
from peft import LoraConfig, get_peft_model
from datasets import load_dataset
from trl import SFTTrainer

# 1. Load Base Model
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
    target_modules=["linear1", "linear2", "lm_head"]
)

# 3. Apply LoRA adapters
model = get_peft_model(model, lora_config)

# 4. Load and format dataset
dataset = load_dataset("json", data_files="training_data_aligned.json")["train"]

def formatting_func(example):
    return {
        "text": f"{example['input']} {example['output']}"
    }

formatted_dataset = dataset.map(formatting_func)

# 5. Tokenize
def tokenize_function(example):
    return tokenizer(example["text"], truncation=True)

tokenized_dataset = formatted_dataset.map(tokenize_function, batched=True)

# 6. Training arguments
training_args = TrainingArguments(
    output_dir="./fine_tuned_phi_v2",
    per_device_train_batch_size=2,
    gradient_accumulation_steps=4,
    num_train_epochs=3,
    logging_steps=10,
    save_steps=100,
    save_total_limit=1,
    fp16=True,
    optim="adamw_torch"
)

# 7. Train model
trainer = SFTTrainer(
    model=model,
    train_dataset=tokenized_dataset,
    args=training_args,
    peft_config=lora_config
)

trainer.train()

# 8. Save model
model.save_pretrained("./fine_tuned_phi_v2")
tokenizer.save_pretrained("./fine_tuned_phi_v2")
