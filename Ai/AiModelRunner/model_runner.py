# model_runner.py
from transformers import AutoTokenizer, AutoModelForCausalLM
import torch

# Load model and tokenizer
model_path = "C:/temp/DevAgeTraining/Ai/AiModelRunner/fine_tuned_phi_v3/"
tokenizer = AutoTokenizer.from_pretrained(model_path)
model = AutoModelForCausalLM.from_pretrained(model_path)

# Input and generation
prompt = "Provide a habit to track only without description.\nHabit ="
inputs = tokenizer(prompt, return_tensors="pt")
outputs = model.generate(**inputs, max_new_tokens=30)

# Decode and write to file
response = tokenizer.decode(outputs[0], skip_special_tokens=True).strip()
response = response.replace("\n", " ").strip()
response = response.replace(prompt, "").strip()

# Flatten and limit to 5 words
words = response.split()
flattened = " ".join(words[:5])

# Save to file
with open("C:/temp/DevAgeTraining/Ai/AiModelRunner/output.txt", "w", encoding="utf-8") as f:
    f.write(flattened)
