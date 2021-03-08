export const footerLinksOrder = ['Questions?', 'About', 'Have thoughts?']

export const socialLinks = {
  discord: "https://discord.gg/b2TMsQF7NG",
  reddit:  "https://www.reddit.com/r/ScrawlBrawl/",
  twitch: "https://www.twitch.tv/scrawlbrawl",
  twitter: "https://twitter.com/ScrawlBrawlGame",
  youtube: "https://www.youtube.com/channel/UCXaY71hm9tbl1xSb-FKfZRA", /* needs correct link */
}

export const footerLinks = {
  'Questions?': [
    // {url: '/', name: "Check out our tutorial"},
    {url: '/questions', name: "FAQ"}
  ],
  'About': [
    {url: '/about', name: "Team"},
    {url: 'https://github.com/kevinjroyston/ScrawlBrawl', name: "Join Us", external: true}
  ],
  'Have thoughts?': [
    {url: '/feedback', name: "Feedback"},
    {url: socialLinks.discord, name: "Contact Us", external: true}
  ]
}
