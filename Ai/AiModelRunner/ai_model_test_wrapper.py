"""
Test wrapper for C# xUnit integration.
Provides a simple stdin/stdout interface specifically designed for automated testing.
Uses the shared ai_model_core for model functionality.
"""

import sys
import json
import warnings

# Import the shared model core
from ai_model_core import get_model_instance


def main():
    """Main entry point for test wrapper."""
    # Suppress all warnings for clean test output
    warnings.filterwarnings('ignore')
    
    try:
        # Initialize the model (singleton)
        model_core = get_model_instance()
        
        # Signal ready status to C#
        print("TEST_READY")
        sys.stdout.flush()
        
        while True:
            try:
                # Read input from stdin
                input_line = sys.stdin.readline().strip()
                
                # Skip empty lines
                if not input_line:
                    continue
                
                # Exit command
                if input_line.lower() == "exit":
                    print("TEST_EXIT")
                    sys.stdout.flush()
                    break
                
                # Check for JSON formatted input (for advanced test scenarios)
                if input_line.startswith("{"):
                    try:
                        data = json.loads(input_line)
                        prompt = data.get("prompt", "")
                        params = data.get("params", {})
                        
                        # Generate response with custom parameters
                        response = model_core.generate_response(prompt, **params)
                        
                        # Return JSON response
                        result = {
                            "status": "success",
                            "response": response
                        }
                        print(json.dumps(result))
                    except json.JSONDecodeError:
                        # Treat as regular text if JSON parsing fails
                        response = model_core.generate_response(input_line)
                        print(response)
                else:
                    # Regular text input
                    response = model_core.generate_response(input_line)
                    print(response)
                
                sys.stdout.flush()
                
            except Exception as e:
                # Return error in a format tests can parse
                error_response = {
                    "status": "error",
                    "error": str(e)
                }
                print(json.dumps(error_response))
                sys.stdout.flush()
                
    except Exception as e:
        # Fatal initialization error
        print(f"INIT_ERROR: {str(e)}")
        sys.stdout.flush()
        sys.exit(1)


if __name__ == "__main__":
    main()