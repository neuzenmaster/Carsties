import NextAuth, { Profile } from "next-auth";
import { OIDCConfig } from "next-auth/providers";
import DuendeIdentityServer6 from "next-auth/providers/duende-identity-server6";

export const { handlers, signIn, signOut, auth } = NextAuth({
  providers: [
    DuendeIdentityServer6({
      id: "id-server",
      clientId: "nextApp",
      clientSecret: "secret",
      issuer: process.env.ID_URL,
      authorization: {
        params: { scope: "openid profile auctionApp" },
        url: process.env.ID_URL + "/connect/authorize",
      },
      token: {
        url: `${process.env.ID_URL_INTERNAL}/connect/token`,
      },
      userinfo: {
        url: `${process.env.ID_URL_INTERNAL}/connect/token`,
      },
      idToken: true,
    } as OIDCConfig<Omit<Profile, "username">>),
  ],
  callbacks: {
    async redirect({ url, baseUrl }) {
      return url.startsWith(baseUrl) ? url : baseUrl;
    },
    jwt({ token, profile, account }) {
      if (account && account.access_token) {
        token.accessToken = account.access_token;
      }
      if (profile) {
        token.username = profile.username;
      }

      return token;
    },
    session({ session, token }) {
      if (token) {
        session.user.username = token.username;
        session.accessToken = token.accessToken;
      }
      return session;
    },
    authorized: async ({ auth }) => {
      return !!auth;
    },
  },
});
