#!/usr/bin/env python3
"""
Fix escape sequence issues in C# files.
Converts $\"string\" to $\"string\" properly.
"""

import os
import re

def fix_csharp_escape_sequences(file_path):
    """Fix escape sequences in a C# file"""
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        original_content = content
        
        # Fix the main issue: $\"string\" should be $\"string\"
        # First fix the escaped quotes
        content = content.replace(r'\"', '"')
        
        # Fix any remaining double backslashes
        content = content.replace(r'\\', '\\')
        
        if content != original_content:
            with open(file_path, 'w', encoding='utf-8') as f:
                f.write(content)
            print(f"Fixed: {file_path}")
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
                if fix_csharp_escape_sequences(file_path):
                    fixed_count += 1
    
    print(f"\nProcessed {total_files} C# files")
    print(f"Fixed {fixed_count} files")

if __name__ == '__main__':
    main()