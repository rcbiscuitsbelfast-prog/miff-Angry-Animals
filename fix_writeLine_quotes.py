#!/usr/bin/env python3
"""
Fix unescaped quotes in WriteLine statements in C# files.
"""

import os
import re

def fix_writeLine_quotes(file_path):
    """Fix unescaped quotes in WriteLine statements"""
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        original_content = content
        
        # Fix patterns like: writer.WriteLine("name="iOS"");
        # Convert to: writer.WriteLine("name=\"iOS\"");
        content = re.sub(
            r'WriteLine\("([^"]*)"([^"]*)"([^"]*)"([^"]*)"([^"]*)"([^)]*)\)',
            r'WriteLine("\\1\"\\2\"\\3\"\\4\"\\5\"\\6")',
            content
        )
        
        # Fix simpler patterns like: "key" : "value" -> \"key\" : \"value\"
        content = re.sub(r'"([^"]+)"\s*:\s*"([^"]+)"', r'\\"\1\\" : \\"\2\\"', content)
        
        # Fix string literals with unescaped quotes
        content = re.sub(r'"([^"]*)"([^"]*)"([^"]*)"', r'"\\1\"\\2\"\\3"', content)
        
        if content != original_content:
            with open(file_path, 'w', encoding='utf-8') as f:
                f.write(content)
            print(f"Fixed WriteLine quotes: {file_path}")
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
                if fix_writeLine_quotes(file_path):
                    fixed_count += 1
    
    print(f"\nProcessed {total_files} C# files")
    print(f"Fixed {fixed_count} files with WriteLine quote issues")

if __name__ == '__main__':
    main()