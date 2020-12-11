import requests
import re


class Api:
    def __init__(self, host: str):
        self.host = host
        self.url = f"http://{self.host}"
        self.api_url = f"{self.url}/api"
        self.session = requests.Session()

    def get_middleware_token(self, html: str) -> str:
        return html.split('csrfmiddlewaretoken" value="')[1].split('"')[0]

    def sing_up(self, username: str, password: str, fname: str, lname:str ) -> requests.Response:
        r = self.session.get(f"{self.url}/signup/")

        r = self.session.post(f"{self.url}/signup/", data={
            "csrfmiddlewaretoken": self.get_middleware_token(r.text),
            "username": username, 
            "password": password, 
            "fname": fname, 
            "lname": lname
        })
        return r

    def login(self, username: str, password: str) -> requests.Response:
        r = self.session.get(f"{self.url}/login/")
        
        r = self.session.post(f"{self.url}/login/", data={
            "csrfmiddlewaretoken": self.get_middleware_token(r.text),
            "username": username, 
            "password": password
        })
        return r

    def add_bark(self, username: str, text: str, is_private: bool = False) -> requests.Response:
        r = self.session.get(f"{self.url}/{username}/")

        data = {
            "csrfmiddlewaretoken": self.get_middleware_token(r.text),
            "bark_text": text
        }

        if is_private:
            data["is_private"] = 1

        r = self.session.post(f"{self.url}/add_bark/", data=data)
        return r

    def get_bark(self, bark_id: int) -> requests.Response:
        r = self.session.get(f"{self.url}/get_bark/{bark_id}/")
        if r.status_code == 200:
            return r

    def comment_bark(self, bark_id: int, text: str, is_private: bool = False) -> requests.Response:
        r = self.session.get(f"{self.url}/get_bark/{bark_id}/")
        
        data = {
            "csrfmiddlewaretoken": self.get_middleware_token(r.text),
            "comment_text": text
        }

        if is_private:
            data["is_private"] = 1
        
        r = self.session.post(f"{self.url}/leave_comment/{bark_id}/", data=data)
        return r
        
    def add_friend(self, username: str) -> requests.Response:
        r = self.session.get(f"{self.url}/add_friend/{username}/")
        return r

    def confirm_friend(self, username: str) -> requests.Response:
        r = self.session.get(f"{self.url}/confirm_friend/{username}/")
        return r

    def generate_token(self) -> str:
        r = self.session.get(f"{self.url}/generate_token/")
        if r.status_code == 200:
            tokens = re.findall(r"[\da-f]{32}", r.text)
            if tokens:
                return tokens[0]
    
    def api_index(self, token) -> dict:
        r = requests.get(f"{self.api_url}/", headers={"Token": token})
        if r.status_code == 200:
            return r.json()

    def api_barks(self, token, username) -> list:
        r = requests.get(f"{self.api_url}/barks/{username}/", headers={"Token": token})
        if r.status_code == 200:
            return r.json()

    def api_comments(self, token, bark_id) -> list:
        r = requests.get(f"{self.api_url}/comments/{bark_id}/", headers={"Token": token})
        if r.status_code == 200:
            return r.json()

    def api_user_info(self, token, username) -> dict:
        r = requests.get(f"{self.api_url}/user/{username}", headers={"Token": token})
        if r.status_code == 200:
            return r.json()

    def api_get_users(self, token, page_n) -> list:
        r = requests.get(f"{self.api_url}/users/{page_n}", headers={"Token": token})
        if r.status_code == 200:
            return r.json()

    def api_get_last_barks(self, token, page_n) -> list:
        r = requests.get(f"{self.api_url}/last_barks/{page_n}/", headers={"Token": token})
        if r.status_code == 200:
            return r.json()

    def logout(self):
        self.session.get(f"{self.url}/logout/")
