import json
import os
import subprocess
import ollama
import time

# Step 1: Create the Training Dataset
dataset_filename = "train.jsonl"

data = [
    {"messages": [{"role": "user", "content": "Please suggest a habit that can be tracked."}, {"role": "assistant", "content": "Track daily water intake."}]},
    {"messages": [{"role": "user", "content": "Please suggest a habit that can be tracked."}, {"role": "assistant", "content": "Exercise for 30 minutes daily."}]},
    {"messages": [{"role": "user", "content": "Please suggest a habit that can be tracked."}, {"role": "assistant", "content": "Write a daily journal entry."}]},
    {"messages": [{"role": "user", "content": "Please suggest a habit that can be tracked."}, {"role": "assistant", "content": "Read 10 pages per day."}]},
    {"messages": [{"role": "user", "content": "Please suggest a habit that can be tracked."}, {"role": "assistant", "content": "Meditate for 10 minutes."}]},
    {"messages": [{"role": "user", "content": "Please suggest a habit that can be tracked."}, {"role": "assistant", "content": "Sleep for 8 hours."}]},
    {"messages": [{"role": "user", "content": "Please suggest a habit that can be tracked."}, {"role": "assistant", "content": "Stretch for 5 minutes."}]},
    {"messages": [{"role": "user", "content": "Please suggest a habit that can be tracked."}, {"role": "assistant", "content": "Practice gratitude daily."}]},
    {"messages": [{"role": "user", "content": "Please suggest a habit that can be tracked."}, {"role": "assistant", "content": "Floss teeth every night."}]},
    {"messages": [{"role": "user", "content": "Please suggest a habit that can be tracked."}, {"role": "assistant", "content": "Avoid social media after 9 PM."}]}
]

# Save dataset to JSONL file
with open(dataset_filename, "w", encoding="utf-8") as f:
    for entry in data:
        f.write(json.dumps(entry) + "\n")

print(f"✅ Dataset created: {dataset_filename}")

# Step 2: Fine-Tune Mistral 7B with Ollama
model_name = "mistral-custom"

print("⏳ Starting fine-tuning...")
fine_tune_command = f"ollama create {model_name} -f {dataset_filename}"

# Run the fine-tuning command
process = subprocess.run(fine_tune_command, shell=True, text=True, capture_output=True)

# Check for errors
if process.returncode != 0:
    print("❌ Fine-tuning failed:", process.stderr)
    exit(1)

print(f"✅ Fine-tuning complete. Model saved as '{model_name}'.")

# Step 3: Test the Fine-Tuned Model
test_prompt = "Please suggest a habit that can be tracked."

print("⏳ Testing the fine-tuned model...")

response = ollama.chat(model=model_name, messages=[{"role": "user", "content": test_prompt}])

print("✅ Model Response:", response['message']['content'])
