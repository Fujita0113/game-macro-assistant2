import os, sys, requests

GITHUB_TOKEN = os.environ["CLAUDE_PAT"]
OWNER = "Fujita0113"
REPO = "game-macro-assistant2"

def create_issue(title, body):
    url = f"https://api.github.com/repos/{OWNER}/{REPO}/issues"
    headers = {"Authorization": f"token {GITHUB_TOKEN}"}
    data = {"title": title, "body": body, "labels": ["ask-codex"]}
    r = requests.post(url, headers=headers, json=data)
    print("Status:", r.status_code, r.json().get("html_url"))

if __name__ == "__main__":
    args = sys.argv
    title = args[args.index("--title")+1] if "--title" in args else "No title"
    body = args[args.index("--body")+1] if "--body" in args else "No body"
    create_issue(title, body)
