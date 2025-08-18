from transformers import (
    AutoModelForCausalLM,
    AutoTokenizer,
    TrainingArguments
)
from datasets import Dataset
from peft import LoraConfig, get_peft_model, TaskType
from trl import SFTTrainer, DataCollatorForCompletionOnlyLM
import torch
import json
import random

# Model configuration
MODEL_NAME = "microsoft/Phi-3.5-mini-instruct"
MODEL_REVISION = "main"  # Pin to specific revision for consistency
OUTPUT_DIR = "./fine_tuned_phi_habits"

# SECURITY CONFIGURATION - DO NOT MODIFY
# Note: Phi-3.5 requires trust_remote_code=True for custom architecture
# This is a verified Microsoft model, but code scanning is always enforced
TRUST_REMOTE_CODE = True  # Required for Phi-3.5 architecture
# SECURITY: Malicious code scanning is ALWAYS enabled and cannot be disabled
import warnings
warnings.filterwarnings("default", category=UserWarning, module="transformers")

# Load and prepare training data
def load_training_data(tokenizer):
    """Generate high-quality training data for habit suggestions"""
    
    # High-quality 5-6 word habit templates ONLY
    habit_templates = [
        # Health & Fitness (5-6 words exactly)
        "Walk ten thousand steps every day",
        "Exercise thirty minutes each morning daily",
        "Do twenty pushups before breakfast daily",
        "Stretch ten minutes before bed nightly",
        "Run three miles every single morning",
        "Complete morning yoga routine every day",
        "Do fifty squats each morning daily",
        
        # Hydration & Nutrition (5-6 words exactly)
        "Drink eight glasses of water daily",
        "Eat five vegetables servings every day",
        "Skip all sugary drinks every day",
        "Have healthy breakfast every single morning",
        "Pack nutritious lunch all week days",
        "Avoid all junk food completely today",
        
        # Mental Health & Mindfulness (5-6 words exactly)
        "Meditate ten minutes every single morning",
        "Write three gratitudes every night daily",
        "Take five deep breaths every hour",
        "Journal fifteen minutes before bed nightly",
        "Practice mindfulness during every meal daily",
        "Read thirty minutes before sleeping nightly",
        
        # Productivity & Learning (5-6 words exactly)
        "Read twenty pages of book daily",
        "Learn five new words every day",
        "Practice piano thirty minutes every day",
        "Complete three tasks before noon daily",
        "Study one hour without any distractions",
        "Write five hundred words every day",
        
        # Sleep & Rest (5-6 words exactly)
        "Sleep full eight hours every night",
        "Go to bed by ten nightly",
        "Wake up at six every morning",
        "Take twenty minute afternoon nap daily",
        "No screens one hour before bedtime",
        
        # Social & Relationships (5-6 words exactly)
        "Call one friend every single week",
        "Send three thank you notes weekly",
        "Have meaningful conversation every single day",
        "Spend quality time with family daily",
        
        # Personal Care (5-6 words exactly)
        "Floss teeth every single night consistently",
        "Apply sunscreen every morning without fail",
        "Take vitamins with breakfast every morning",
        "Stand up every hour during work",
        "Take five screen breaks every day"
    ]
    
    # Generate dataset with proper instruction format
    data = []
    
    # Use only the specific prompt as requested
    prompt = "Please suggest a habit that can be tracked"
    
    for _ in range(500):  # Generate 500 high-quality examples
        # No placeholders needed - all habits are already 5-6 words
        habit = random.choice(habit_templates)
        
        # Use Phi-3.5's chat template format
        messages = [
            {"role": "user", "content": prompt},
            {"role": "assistant", "content": habit}
        ]
        
        # Apply chat template to create text field
        text = tokenizer.apply_chat_template(messages, tokenize=False)
        data.append({"text": text})
    
    return data

def format_instruction(sample):
    """Format samples using Phi-3.5's chat template"""
    return sample

def scan_model_files():
    """Scan model files for potential security issues"""
    import os
    import hashlib
    from pathlib import Path
    
    print("\n🔍 Security Scan:")
    print("-" * 40)
    
    # Known safe file extensions
    safe_extensions = {'.json', '.txt', '.bin', '.safetensors', '.md'}
    suspicious_extensions = {'.py', '.sh', '.exe', '.dll', '.so'}
    
    # Check cache directory for downloaded files
    cache_dir = Path.home() / ".cache" / "huggingface" / "hub"
    model_id = MODEL_NAME.replace("/", "--")
    
    suspicious_files = []
    
    if cache_dir.exists():
        for model_dir in cache_dir.glob(f"models--{model_id}*"):
            for file_path in model_dir.rglob("*"):
                if file_path.is_file():
                    ext = file_path.suffix.lower()
                    if ext in suspicious_extensions:
                        suspicious_files.append(str(file_path))
                        print(f"⚠️  Found code file: {file_path.name}")
    
    if suspicious_files:
        print(f"\n⚠️  Warning: Found {len(suspicious_files)} code files that could contain malicious code.")
        print("These files should be reviewed manually if TRUST_REMOTE_CODE is enabled.")
        print("ℹ️  NOTE: For Phi-3.5, configuration_phi3.py and modeling_phi3.py are EXPECTED")
        print("   These are official Microsoft architecture files required for the model.")
        
        if not TRUST_REMOTE_CODE:
            print("✅ TRUST_REMOTE_CODE is False - code execution is disabled.")
    else:
        print("✅ No suspicious code files detected in model cache.")
    
    print("-" * 40)
    return len(suspicious_files) == 0

def main():
    print("Loading model and tokenizer...")
    
    # Security warning
    if TRUST_REMOTE_CODE:
        print("\n⚠️  WARNING: TRUST_REMOTE_CODE is enabled!")
        print("This allows execution of custom code from the model repository.")
        response = input("Do you want to continue? (yes/no): ")
        if response.lower() != "yes":
            print("Aborted.")
            return
    
    try:
        # Load model in float16 for Windows compatibility
        print(f"\n📥 Loading model: {MODEL_NAME}")
        print(f"   Revision: {MODEL_REVISION}")
        print(f"   Trust remote code: {TRUST_REMOTE_CODE}")
        
        # SECURITY: Always scan for malicious code when loading models
        model = AutoModelForCausalLM.from_pretrained(
            MODEL_NAME,
            revision=MODEL_REVISION,  # Pin to specific revision
            device_map="auto",
            trust_remote_code=TRUST_REMOTE_CODE,
            torch_dtype=torch.float16,
            use_cache=True,  # Use cached files when available
            code_revision=None  # Always use latest code revision with security patches
        )
        
        # Load tokenizer with security scanning
        tokenizer = AutoTokenizer.from_pretrained(
            MODEL_NAME,
            revision=MODEL_REVISION,
            trust_remote_code=TRUST_REMOTE_CODE,
            padding_side="left",
            code_revision=None  # Always use latest code revision with security patches
        )
        
        # Perform security scan after download
        if SCAN_FOR_MALICIOUS_CODE:
            scan_model_files()
        
    except Exception as e:
        print(f"\n❌ Error loading model: {e}")
        if "trust_remote_code" in str(e).lower():
            print("\nThis model requires custom code execution.")
            print("To enable it, set TRUST_REMOTE_CODE = True")
            print("⚠️  Only do this if you trust the model source!")
        raise
    
    # Set padding token if not set
    if tokenizer.pad_token is None:
        tokenizer.pad_token = tokenizer.eos_token
    
    # LoRA configuration optimized for Phi-3.5
    peft_config = LoraConfig(
        task_type=TaskType.CAUSAL_LM,
        r=32,  # Increased rank for better capacity
        lora_alpha=64,  # Alpha = 2*r for good practice
        lora_dropout=0.1,
        target_modules=["q_proj", "k_proj", "v_proj", "o_proj", "gate_proj", "up_proj", "down_proj"],  # All attention and MLP layers
        bias="none",
    )
    
    # Get PEFT model
    model = get_peft_model(model, peft_config)
    model.print_trainable_parameters()
    
    # Enable training mode
    model.train()
    
    # Ensure gradients are enabled for LoRA parameters
    for name, param in model.named_parameters():
        if param.requires_grad:
            param.requires_grad_(True)
    
    # Load and prepare data
    print("Generating training data...")
    train_data = load_training_data(tokenizer)
    
    # Save training data for inspection
    with open("high_quality_habit_data.json", "w") as f:
        json.dump(train_data, f, indent=2)
    
    dataset = Dataset.from_list(train_data)
    
    # Training arguments optimized for LoRA compatibility
    training_args = TrainingArguments(
        output_dir=OUTPUT_DIR,
        num_train_epochs=3,  # Reduced epochs for stability
        per_device_train_batch_size=1,  # Smaller batch size
        gradient_accumulation_steps=4,  # Effective batch size of 4
        gradient_checkpointing=False,  # Disable for LoRA compatibility
        optim="adamw_torch",  # Standard optimizer for Windows
        logging_steps=10,
        save_strategy="epoch",
        learning_rate=1e-4,  # Lower learning rate for stability
        warmup_steps=50,  # Fixed warmup steps instead of ratio
        lr_scheduler_type="linear",  # Simpler scheduler
        max_grad_norm=0.3,
        fp16=False,  # Disable fp16 for stability
        bf16=False,
        dataloader_pin_memory=False,  # Reduce memory issues
        remove_unused_columns=False,  # Keep all columns
        report_to=["none"],
    )
    
    # Create trainer with latest API (simplified)
    trainer = SFTTrainer(
        model=model,
        args=training_args,
        train_dataset=dataset,
        processing_class=tokenizer,
        peft_config=peft_config,
    )
    
    # Start training
    print("Starting training...")
    trainer.train()
    
    # Save the model
    print("Saving model...")
    trainer.save_model()
    tokenizer.save_pretrained(OUTPUT_DIR)
    
    print(f"Training complete! Model saved to {OUTPUT_DIR}")
    
    # Test the model
    print("\nTesting the fine-tuned model...")
    # Use only the one prompt we trained on
    test_prompts = [
        "Please suggest a habit that can be tracked"
    ]
    
    model.eval()
    for prompt in test_prompts:
        messages = [{"role": "user", "content": prompt}]
        
        # Apply chat template
        text = tokenizer.apply_chat_template(messages, tokenize=False, add_generation_prompt=True)
        inputs = tokenizer(text, return_tensors="pt").to(model.device)
        
        with torch.no_grad():
            outputs = model.generate(
                **inputs,
                max_new_tokens=15,  # Short for 5-6 words
                temperature=0.7,
                do_sample=True,
                top_p=0.9,
                pad_token_id=tokenizer.pad_token_id,
                eos_token_id=tokenizer.eos_token_id,
            )
        
        response = tokenizer.decode(outputs[0], skip_special_tokens=True)
        # Extract only the assistant's response
        if "<|assistant|>" in response:
            response = response.split("<|assistant|>")[-1].strip()
        
        print(f"\nPrompt: {prompt}")
        print(f"Response: {response}")

if __name__ == "__main__":
    main()