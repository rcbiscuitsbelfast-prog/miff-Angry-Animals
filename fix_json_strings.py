#!/usr/bin/env python3
"""
Fix JSON string literal issues in C# files.
Converts unescaped quotes in string literals to properly escaped quotes.
"""

import os
import re

def fix_json_strings_in_csharp(file_path):
    """Fix JSON string literal issues in a C# file"""
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        original_content = content
        
        # Pattern to match strings that contain unescaped quotes (common JSON pattern)
        # Look for patterns like "  "key" : "value"" within WriteLine calls
        # This regex finds strings that contain unescaped quotes within the content
        json_pattern = r'(WriteLine\([^)]*")([^"]*)"([^"]*)"([^"]*)"([^)]*\))'
        
        def replace_json_quotes(match):
            prefix = match.group(1)  # WriteLine("
            part1 = match.group(2)   #   or other content before first quote
            key = match.group(3)     # key name
            middle = match.group(4)  #  : 
            value = match.group(5)   # value
            suffix = match.group(6)  # ")
            
            # Escape the quotes around the key and value
            fixed_key = f'\\"{key}\\"'
            # Only escape if there's actually a value part
            fixed_value = f'\\"{value}\\"' if value else '"'
            
            return f'{prefix}{part1}{fixed_key}{middle}{fixed_value}{suffix}'
        
        # Apply the fix multiple times to handle nested patterns
        iterations = 0
        max_iterations = 10
        while iterations < max_iterations:
            new_content = re.sub(json_pattern, replace_json_quotes, content)
            if new_content == content:
                break
            content = new_content
            iterations += 1
        
        # Also fix any remaining obvious JSON quote issues
        # Pattern: "key" : "value" -> \"key\" : \"value\"
        content = re.sub(r'"([^"]+)"\s*:\s*"([^"]+)"', r'\\"\1\\" : \\"\2\\"', content)
        
        # Fix WriteLine calls with JSON fragments
        content = re.sub(r'WriteLine\("([^"]*)"([^"]*)"([^"]*)"([^"]*)"([^"]*)"([^)]*)\)', 
                        lambda m: f'WriteLine("{m.group(1)}\\\"{m.group(2)}\\\"{m.group(3)}\\\"{m.group(4)}\\\"{m.group(5)}\\\"{m.group(6)})',
                        content)
        
        if content != original_content:
            with open(file_path, 'w', encoding='utf-8') as f:
                f.write(content)
            print(f"Fixed JSON strings: {file_path}")
            return True
        else:
            return False
            
    except Exception as e:
        print(f"Error processing {file_path}: {e}")
        return False

def main():
    """Process all C# files in the project"""
    fixed_count = 0
    total_files = 0
    
    for root, dirs, files in os.walk('.'):
        # Skip .git directory
        if '.git' in root:
            continue
            
        for file in files:
            if file.endswith('.cs'):
                file_path = os.path.join(root, file)
                total_files += 1
                if fix_json_strings_in_csharp(file_path):
                    fixed_count += 1
    
    print(f"\nProcessed {total_files} C# files")
    print(f"Fixed {fixed_count} files with JSON string issues")

if __name__ == '__main__':
    main()