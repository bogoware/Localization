#!/usr/bin/env node

/**
 * Syncs CHANGELOG.md from the project root to Docusaurus docs folder.
 * Adds Docusaurus frontmatter to make it compatible with the documentation site.
 */

const fs = require('fs');
const path = require('path');

const ROOT_DIR = path.join(__dirname, '..');
const DOCS_DIR = path.join(ROOT_DIR, 'docs', 'docs');

/**
 * Adds frontmatter to markdown content
 */
function addFrontmatter(content, frontmatter) {
    const frontmatterStr = Object.entries(frontmatter)
        .map(([key, value]) => `${key}: ${value}`)
        .join('\n');
    return `---\n${frontmatterStr}\n---\n\n${content}`;
}

/**
 * Transforms CHANGELOG.md for Docusaurus
 */
function transformChangelog(content) {
    return content;
}

// Files to sync
const filesToSync = [
    {
        source: path.join(ROOT_DIR, 'CHANGELOG.md'),
        dest: path.join(DOCS_DIR, 'changelog.md'),
        frontmatter: {
            sidebar_position: 2,
            title: 'Changelog'
        },
        transform: transformChangelog
    }
];

// Process each file
for (const file of filesToSync) {
    try {
        if (!fs.existsSync(file.source)) {
            console.warn(`Warning: Source file not found: ${file.source}`);
            continue;
        }

        let content = fs.readFileSync(file.source, 'utf8');

        if (file.transform) {
            content = file.transform(content);
        }

        content = addFrontmatter(content, file.frontmatter);

        fs.writeFileSync(file.dest, content, 'utf8');
        console.log(`Synced: ${path.basename(file.source)} -> ${path.relative(ROOT_DIR, file.dest)}`);
    } catch (error) {
        console.error(`Error syncing ${file.source}: ${error.message}`);
        process.exit(1);
    }
}

console.log('Sync complete!');
