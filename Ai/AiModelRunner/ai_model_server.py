import sys
import os
import warnings
import logging

# Suppress all warnings
warnings.filterwarnings('ignore')
os.environ['TF_CPP_MIN_LOG_LEVEL'] = '3'

# Suppress transformers and other library logging
logging.getLogger('transformers').setLevel(logging.ERROR)
logging.getLogger('transformers.modeling_utils').setLevel(logging.ERROR) 
logging.getLogger('transformers.tokenization_utils').setLevel(logging.ERROR)
logging.getLogger('peft').setLevel(logging.ERROR)
logging.getLogger('bitsandbytes').setLevel(logging.ERROR)

# Suppress stderr output during imports
import contextlib
import io

# Now import transformers after setting up suppression
with contextlib.redirect_stderr(io.StringIO()):
    from transformers import AutoTokenizer, AutoModelForCausalLM
    from peft import PeftModel
    import torch
    
    # Disable transformers warnings about deprecations
    import transformers
    transformers.logging.set_verbosity_error()

# Flask imports
from flask import Flask, request, jsonify
from flask_cors import CORS

# Model configuration
model_name = "microsoft/Phi-3.5-mini-instruct"
adapter_path = "./fine_tuned_phi_habits"  # Path to your fine-tuned adapters

print("Loading tokenizer...")
# Load the tokenizer
with warnings.catch_warnings():
    warnings.simplefilter("ignore")
    tokenizer = AutoTokenizer.from_pretrained(model_name)

print("Loading base model...")
# Load the base model
# SECURITY: Always scan for malicious code when loading models
with warnings.catch_warnings():
    warnings.simplefilter("ignore")
    # Redirect stderr temporarily to suppress loading bars
    import contextlib
    import io
    with contextlib.redirect_stderr(io.StringIO()):
        model = AutoModelForCausalLM.from_pretrained(
            model_name,
            torch_dtype=torch.float32,  # Force float32 for CPU
            device_map="cpu",  # Force CPU to avoid memory issues
            low_cpu_mem_usage=True,  # Enable memory optimization
            trust_remote_code=True,  # Required for Phi-3.5
            code_revision=None  # Always use latest code revision with security patches
        )

print("Loading LoRA adapters...")
# Load the fine-tuned LoRA adapters if they exist
if os.path.exists(adapter_path):
    with warnings.catch_warnings():
        warnings.simplefilter("ignore")
        model = PeftModel.from_pretrained(model, adapter_path)
    print("LoRA adapters loaded successfully")
else:
    print("No LoRA adapters found, using base model")

def generate_response(input_line):
    try:
        # For the fine-tuned model, we use the exact prompt it was trained on
        if "suggest a habit" in input_line.lower():
            # Use the exact format from training
            messages = [
                {"role": "user", "content": input_line}
            ]
        else:
            # For other queries, add a system message
            messages = [
                {
                    "role": "system", 
                    "content": "You are a helpful habit tracking assistant. Provide concise, actionable responses."
                },
                {"role": "user", "content": input_line}
            ]
        
        # Apply the chat template
        prompt = tokenizer.apply_chat_template(
            messages, 
            tokenize=False, 
            add_generation_prompt=True
        )
        
        # Encode the input text
        inputs = tokenizer(prompt, return_tensors="pt", truncation=True, max_length=512)
        
        # Ensure model is running on the correct device (CPU/GPU)
        inputs = {k: v.to(model.device) for k, v in inputs.items()}
            
        # Generate a response with parameters optimized for the fine-tuned model
        with warnings.catch_warnings():
            warnings.simplefilter("ignore")
            with torch.no_grad():
                outputs = model.generate(
                    **inputs,
                    max_new_tokens=10,  # Enough for 5-6 word habits
                    do_sample=True,     # Enable sampling for variety
                    temperature=0.5,    # Moderate temperature for good variety
                    top_p=0.9,          # Standard nucleus sampling
                    pad_token_id=tokenizer.pad_token_id,
                    eos_token_id=tokenizer.eos_token_id
                )

        # Decode only the generated tokens (exclude input)
        generated_tokens = outputs[0][len(inputs['input_ids'][0]):]
        response = tokenizer.decode(generated_tokens, skip_special_tokens=True).strip()
        
        # Clean up the response
        if response:
            # Remove any quotation marks if present
            response = response.strip('"\'')
            
            # Take only the first line if there are multiple
            if '\n' in response:
                response = response.split('\n')[0].strip()
            
            # Clean up any incomplete sentences
            # If it doesn't end with punctuation and has more than 3 words, add a period
            if response and not response[-1] in '.!?':
                words = response.split()
                if len(words) >= 3:
                    response = response + '.'
            
            response = response.strip()
        
        return response

    except Exception as e:
        return f"An error occurred: {str(e)}"

# Create Flask app
app = Flask(__name__)
CORS(app)  # Enable CORS for cross-origin requests

@app.route('/health', methods=['GET'])
def health_check():
    """Health check endpoint"""
    return jsonify({"status": "ready", "model": model_name})

@app.route('/chat', methods=['POST'])
def chat():
    """Chat endpoint that processes messages and returns AI responses"""
    try:
        data = request.get_json()
        if not data or 'message' not in data:
            return jsonify({"error": "Missing 'message' field"}), 400
        
        message = data['message']
        if not message.strip():
            return jsonify({"error": "Message cannot be empty"}), 400
        
        # Generate response using your fine-tuned model
        response = generate_response(message)
        
        return jsonify({
            "response": response,
            "model": model_name,
            "status": "success"
        })
        
    except Exception as e:
        return jsonify({"error": str(e)}), 500

if __name__ == '__main__':
    print("AI Model Server starting...")
    print(f"Model: {model_name}")
    print(f"LoRA adapters: {'Found' if os.path.exists(adapter_path) else 'Not found'}")
    print("Server will be available at: http://localhost:5000")
    print("Health check: http://localhost:5000/health")
    print("Chat endpoint: POST http://localhost:5000/chat")
    
    # Start the Flask server
    app.run(host='localhost', port=5000, debug=False, threaded=True)