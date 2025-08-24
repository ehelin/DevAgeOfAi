"""
Flask API server for the AI model.
Uses the shared ai_model_core for model functionality.
"""

import os
import sys
from flask import Flask, request, jsonify
from flask_cors import CORS

# Import the shared model core
from ai_model_core import get_model_instance

# Create Flask app
app = Flask(__name__)
CORS(app)  # Enable CORS for cross-origin requests

# Initialize the model (singleton - loaded once)
print("Initializing AI Model for Flask server...")
model_core = get_model_instance()


@app.route('/health', methods=['GET'])
def health_check():
    """Health check endpoint"""
    model_info = model_core.get_model_info()
    return jsonify({
        "status": "ready",
        "model": model_info["model_name"],
        "adapter_loaded": model_info["adapter_loaded"]
    })


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
        
        # Optional: Allow client to override generation parameters
        gen_params = {}
        if 'temperature' in data:
            gen_params['temperature'] = data['temperature']
        if 'max_new_tokens' in data:
            gen_params['max_new_tokens'] = data['max_new_tokens']
        if 'top_p' in data:
            gen_params['top_p'] = data['top_p']
        
        # Generate response using the model core
        response = model_core.generate_response(message, **gen_params)
        
        return jsonify({
            "response": response,
            "model": model_core.model_name,
            "status": "success"
        })
        
    except Exception as e:
        return jsonify({"error": str(e)}), 500


@app.route('/model-info', methods=['GET'])
def model_info():
    """Get information about the loaded model"""
    return jsonify(model_core.get_model_info())


if __name__ == '__main__':
    print("AI Model Server starting...")
    model_info = model_core.get_model_info()
    print(f"Model: {model_info['model_name']}")
    print(f"LoRA adapters: {'Loaded' if model_info['adapter_loaded'] else 'Not found'}")
    print(f"Device: {model_info['device']}")
    print("\nServer will be available at: http://localhost:5000")
    print("Endpoints:")
    print("  - Health check: GET http://localhost:5000/health")
    print("  - Chat: POST http://localhost:5000/chat")
    print("  - Model info: GET http://localhost:5000/model-info")
    print("\nPress Ctrl+C to stop the server")
    
    # Start the Flask server
    app.run(host='localhost', port=5000, debug=False, threaded=True)