#!/usr/bin/env python3
"""
Fix remaining escape sequence issues in C# files.
"""

import os
import re

def fix_remaining_escape_sequences(file_path):
    """Fix remaining escape sequence issues"""
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        original_content = content
        
        # Fix all escape sequences that got corrupted
        content = content.replace(r'\"', '"')
        content = content.replace(r'\\', '\\')
        
        # Fix any remaining patterns
        content = re.sub(r'\$\\"([^\\]+)\\"', r'$\1', content)
        content = re.sub(r'\\"([^"]+)\\"', r'"\1"', content)
        
        if content != original_content:
            with open(file_path, 'w', encoding='utf-8') as f:
                f.write(content)
            print(f"Fixed escape sequences: {file_path}")
            return True
        else:
            return False
            
    except Exception as e:
        print(f"Error processing {file_path}: {e}")
        return False

def main():
    """Process problematic files"""
    problematic_files = [
        './Scripts/Levels/GenerateAllLevels.cs',
        './Scripts/Levels/ProceduralRoom.cs',
        './Scripts/Levels/ProceduralLevelGenerator.cs'
    ]
    
    fixed_count = 0
    
    for file_path in problematic_files:
        if os.path.exists(file_path):
            if fix_remaining_escape_sequences(file_path):
                fixed_count += 1
    
    print(f"Fixed {fixed_count} files")

if __name__ == '__main__':
    main()