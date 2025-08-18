# Habit Suggestion Model - Setup and Training Guide

## Overview
This guide explains how to download the base Phi-3.5 model and fine-tune it for habit suggestions.

## Prerequisites

### Required Python Packages
```bash
pip install transformers datasets peft trl torch accelerate bitsandbytes
```

### Hardware Requirements
- **Minimum**: 16GB RAM (CPU training, slower)
- **Recommended**: NVIDIA GPU with 8GB+ VRAM (much faster)
- **Disk Space**: ~15GB for model and training artifacts

## Process to Re-train from Scratch

### 1. Clean Previous Training (if exists)
If you need to start fresh, delete these folders:
```bash
# From the AiModelRunner directory
rm -rf __pycache__
rm -rf fine_tuned_phi_habits
rm -rf fine_tuned_phi_v3
rm -rf model_cache  # Optional: delete cached base model
```

### 2. Run the Complete Setup and Training
```bash
cd C:\temp\DevAgeTraining\Ai\AiModelRunner
python setup_and_train.py
```

This script will:
1. **Download the base model** (~7GB) from Hugging Face
2. **Prepare training data** (440 examples of 5-6 word habits)
3. **Fine-tune using QLoRA** (takes 15-30 minutes)
4. **Test the model** with sample prompts
5. **Save the adapters** to `./fine_tuned_phi_habits`

### 3. Alternative: Use Original Training Script
If you prefer to use the existing `qlora_train.py`:
```bash
python qlora_train.py
```

## What Gets Created

After training, you'll have:
```
fine_tuned_phi_habits/
├── adapter_config.json       # LoRA configuration
├── adapter_model.safetensors # Fine-tuned weights (small, ~10MB)
├── tokenizer_config.json     # Tokenizer settings
├── special_tokens_map.json   # Special tokens
└── checkpoint-*/             # Training checkpoints
```

## How the Model Works

1. **Base Model**: Microsoft Phi-3.5-mini-instruct (3.8B parameters)
2. **Fine-tuning Method**: QLoRA (Quantized Low-Rank Adaptation)
   - Only trains ~0.1% of parameters
   - Keeps base model frozen
   - Adds small adapter layers
3. **Training Data**: 440 examples of habit suggestions in 5-6 word format
4. **Output**: Concise, trackable habit suggestions

## Testing the Model

### Interactive Test
```bash
python AiModelServiceRunner.py
```
Then type: `Please suggest a habit that can be tracked`

### Batch Test
```bash
python test_model.py
```

## Expected Output Examples
- "Walk ten thousand steps every day"
- "Drink eight glasses of water daily"  
- "Meditate ten minutes every single morning"
- "Read twenty pages of book daily"
- "Exercise thirty minutes each morning daily"

## Troubleshooting

### Out of Memory Errors
- Reduce `per_device_train_batch_size` in setup_and_train.py
- Increase `gradient_accumulation_steps` to compensate
- Use CPU training (slower but works with less memory)

### Model Not Loading Adapters
Ensure `AiModelServiceRunner.py` has these lines:
```python
from peft import PeftModel
# ... 
if os.path.exists(adapter_path):
    model = PeftModel.from_pretrained(model, adapter_path)
```

### Poor Quality Outputs
- The model might need more training epochs
- Check that training data quality is good
- Adjust temperature in generation (0.3-0.7 works best)

## Integration with C# Application

The trained model integrates with the C# habit tracker app through:
1. `AiModelServiceRunner.py` loads the fine-tuned model
2. C# `PythonScriptService` communicates via stdin/stdout
3. The app receives habit suggestions from the model

## Notes

- Training takes 15-30 minutes on a decent GPU
- The fine-tuned adapters are only ~10MB (vs 7GB base model)
- The model is specifically optimized for 5-6 word habit suggestions
- You can modify habit templates in the training script for different styles