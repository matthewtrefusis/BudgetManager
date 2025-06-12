# Git Guidelines for BudgetManager

## Handling Large Files

GitHub has a file size limit of 100MB. To avoid issues when pushing to GitHub, follow these guidelines:

### Best Practices

1. **Never commit binary files larger than 50MB**: 
   - Executable files (.exe)
   - Installation packages
   - Database backups
   - Large media files

2. **Use .gitignore properly**:
   - The project's `.gitignore` already excludes common large binary files
   - If you need to add new file types to ignore, update the `.gitignore` file

3. **For large files that need version control**:
   - Consider using [Git LFS (Large File Storage)](https://git-lfs.github.com/)
   - Git LFS stores large files separately from the Git repository

### What to do if you accidentally commit a large file

If you've already committed a large file but haven't pushed:

```bash
# Remove the file from the last commit but keep it on your filesystem
git reset --soft HEAD~1
git reset HEAD path/to/large/file
git commit -m "Commit message without the large file"

# Update .gitignore to exclude this file in the future
echo "path/to/large/file" >> .gitignore
git add .gitignore
git commit -m "Update .gitignore to exclude large file"
```

If the large file is already in your repository history:

1. Use tools like BFG Repo-Cleaner or git-filter-branch (as we did previously)
2. Always create a backup before cleaning history
3. Inform teammates about the history rewrite, as they'll need to reclone or rebase

### Alternative Storage Solutions for Large Files

- Store compiled binaries in a release section on GitHub
- Use cloud storage solutions for large assets
- Consider package managers for distributing compiled binaries

Remember: Git is designed for source code, not binary assets or build artifacts.
