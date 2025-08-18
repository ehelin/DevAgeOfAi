from transformers import AutoModelForCausalLM, AutoTokenizer
import torch

# Load model to inspect structure
model_name = "microsoft/Phi-3.5-mini-instruct"

print("Loading model to check structure...")
tokenizer = AutoTokenizer.from_pretrained(model_name, trust_remote_code=True)
model = AutoModelForCausalLM.from_pretrained(
    model_name,
    trust_remote_code=True,
    torch_dtype=torch.float32,
    device_map="cpu"  # Load on CPU to save memory
)

print("\nModel structure:")
print("=" * 50)

# Print all named modules to find the correct target modules
for name, module in model.named_modules():
    if any(keyword in name.lower() for keyword in ['proj', 'attn', 'mlp', 'linear']):
        print(f"{name}: {type(module).__name__}")

print("\n" + "=" * 50)
print("Looking for attention and MLP layers specifically:")

# Find specific layer patterns
attention_modules = []
mlp_modules = []

for name, module in model.named_modules():
    if 'attn' in name.lower() and ('proj' in name.lower() or 'linear' in name.lower()):
        attention_modules.append(name)
    elif 'mlp' in name.lower() and ('proj' in name.lower() or 'linear' in name.lower()):
        mlp_modules.append(name)

print("\nAttention modules:")
for module in attention_modules[:10]:  # Show first 10
    print(f"  {module}")

print("\nMLP modules:")
for module in mlp_modules[:10]:  # Show first 10
    print(f"  {module}")

# Extract just the layer type names (without layer numbers)
unique_attention = set()
unique_mlp = set()

for name in attention_modules:
    # Extract the last part (e.g., "qkv_proj" from "model.layers.0.self_attn.qkv_proj")
    layer_type = name.split('.')[-1]
    unique_attention.add(layer_type)

for name in mlp_modules:
    layer_type = name.split('.')[-1]
    unique_mlp.add(layer_type)

print(f"\nUnique attention layer types: {unique_attention}")
print(f"Unique MLP layer types: {unique_mlp}")

print(f"\nRecommended LoRA target_modules:")
all_targets = list(unique_attention) + list(unique_mlp)
print(f"target_modules={all_targets}")