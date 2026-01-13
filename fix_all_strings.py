#!/usr/bin/env python3
"""
Comprehensive fix for string literal issues in C# files.
Fixes escape sequences, WriteLine JSON strings, and other common issues.
"""

import os
import re

def fix_all_string_issues(file_path):
    """Fix all string literal issues in a C# file"""
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        original_content = content
        
        # Fix escape sequence issues first
        content = content.replace(r'\"', '"')
        content = content.replace(r'\\', '\\')
        
        # Fix WriteLine statements with JSON-like strings
        # Pattern: WriteLine("key="value"")
        content = re.sub(
            r'(WriteLine\(")([^"]*)"([^"]*)"([^"]*)"([^"]*)"([^"]*)',
            r'\1\2\"\\3\"\\4\"\\5\"\\6',
            content
        )
        
        # Fix WriteLine with three parts
        content = re.sub(
            r'(WriteLine\(")([^"]*)"([^"]*)"([^"]*)',
            r'\1\2\"\\3\"\\4',
            content
        )
        
        # Fix WriteLine with two parts  
        content = re.sub(
            r'(WriteLine\(")([^"]*)"([^"]*)',
            r'\1\2\"\\3',
            content
        )
        
        # Fix string interpolation that got corrupted
        # Pattern: $"arcade_{levelNumber:D3}" -> $"arcade_{levelNumber:D3}"
        content = re.sub(
            r'\$\\"([^\\]+)\\{([^}]+)\\}\\"',
            r'$\1{\2}',
            content
        )
        
        # Fix any remaining obvious patterns
        # Pattern: "key" : "value" -> "key" : "value"  
        content = re.sub(r'"([^"]+)"\s*:\s*"([^"]+)"', r'\"\\1\" : \"\\2\"', content)
        
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
        # Skip .git directory and build artifacts
        if '.git' in root or 'bin' in root or 'obj' in root:
            continue
            
        for file in files:
            if file.endswith('.cs'):
                file_path = os.path.join(root, file)
                total_files += 1
                if fix_all_string_issues(file_path):
                    fixed_count += 1
    
    print(f"\nProcessed {total_files} C# files")
    print(f"Fixed {fixed_count} files")

if __name__ == '__main__':
    main()