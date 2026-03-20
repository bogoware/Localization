import { defineConfig } from 'astro/config';
import starlight from '@astrojs/starlight';
import bogowareTheme from '@bogoware/starlight-theme';

export default defineConfig({
  site: 'https://bogoware.github.io',
  base: '/Localization/',
  integrations: [
    starlight({
      title: 'Bogoware.Localization',
      tagline: 'Lightweight, FQDN-keyed localization for .NET',
      plugins: [
        bogowareTheme({
          mode: 'architect',
          logoVariant: 'simplified',
          fieldLines: true,
          seo: {
            siteName: 'Bogoware.Localization',
            defaultDescription: 'Lightweight, FQDN-keyed localization for .NET',
            structuredData: { type: 'SoftwareSourceCode', author: 'Bogoware' },
          },
        }),
      ],
      social: {
        github: 'https://github.com/bogoware/Localization',
      },
      editLink: {
        baseUrl: 'https://github.com/bogoware/Localization/tree/rel/prod/website/',
      },
      sidebar: [
        { label: 'Introduction', slug: '' },
        { label: 'Changelog', slug: 'changelog' },
        {
          label: 'Getting Started',
          autogenerate: { directory: 'getting-started' },
        },
        {
          label: 'Concepts',
          autogenerate: { directory: 'concepts' },
        },
        {
          label: 'Guides',
          autogenerate: { directory: 'guides' },
        },
        {
          label: 'API Reference',
          autogenerate: { directory: 'api' },
        },
      ],
    }),
  ],
});
