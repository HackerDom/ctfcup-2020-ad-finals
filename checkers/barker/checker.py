#!/usr/bin/env python3.7

from gornilo import Checker, CheckRequest, PutRequest, GetRequest, Verdict
from api import Api
from requests.exceptions import RequestException
import string, random


checker = Checker()


def get_random_str(lenght=10):
    return "".join(random.choices(string.ascii_letters+string.digits, k=lenght))


def get_random_text_with_lenght(lenght=5):
    return " ".join([get_random_str(random.randint(3,10)) for _ in range(lenght)])


def get_random_text(s=1, e=10):
    return get_random_text_with_lenght(random.randint(s,e))


@checker.define_check
def check(check_request: CheckRequest) -> Verdict:
    api = Api(check_request.hostname)
    try:
        username = get_random_str()
        password = get_random_str()
        r = api.sing_up(username, password, get_random_str(), get_random_str())
        if r.status_code == 200 and f"{api.url}/{username}/" != r.url:
            print(f"Found {r.url}, but wait {api.url}/{username}. Status code {r.status_code}")
            return Verdict.MUMBLE("can't pass registration")
        
        bark = get_random_text()
        r = api.add_bark(username, bark)
        if r.status_code == 200 and f"{api.url}/{username}/" != r.url or bark not in r.text:
            print(f"Found {r.url}, but wait {api.url}/{username} OR '{bark}' not in response. Status code: {r.status_code}")
            return Verdict.MUMBLE("can't create bark")

        bark_id = int(r.text.split(bark)[0].split("/get_bark/")[1][0:-3])
        comment = get_random_text()
        r = api.comment_bark(bark_id, comment)
        if r.status_code == 200 and comment not in r.text:
            print(f"Comment {comment} not in response. Status code: {r.status_code}")
            return Verdict.MUMBLE("can't create comment")
        
        api.logout()

        new_username = get_random_str()
        new_password = get_random_str()
        r = api.sing_up(new_username, new_password, get_random_str(), get_random_str())
        r = api.add_friend(username)

        if r.status_code == 200 and username not in r.text:
            print(f"Can't find {username} in friends list. Status code: {r.status_code}")
            return Verdict.MUMBLE("can't add friend")
        
        api.logout()

        r = api.login(username, password)
        if r.status_code == 200 and f"{api.url}/{username}/" != r.url:
            print(f"Found {r.url}, but wait {api.url}/{username}. Status code: {r.status_code}")
            return Verdict.MUMBLE("can't log in")

        r = api.confirm_friend(new_username)
        if r.status_code == 200 and username not in r.text:
            print(f"Friend {username} not found in friends list. Status code: {r.status_code}")
            return Verdict.MUMBLE("can't confirm friend")

        token = api.generate_token()
        if not token:
            print(f"Can't get token")
            return Verdict.MUMBLE("can't get token")

        api.logout()

        user_dict = api.api_index(token)
        if user_dict['username'] != username or user_dict['token'] != token:
            print(f"Fields username and token isn't correct. Found: {user_dict['username']}, {user_dict['token']}. Wait: {username}, {token}")
            return Verdict.MUMBLE("api user info incorrect")

        barks_list = api.api_barks(token, username)
        for user_bark in barks_list:
            if user_bark['text'] == bark:
                break
        else:
            print(f"Wait for '{bark}', but got {user_bark['text']}")
            return Verdict.MUMBLE("incorrect bark")

        comments_list = api.api_comments(token, bark_id)
        for user_comment in comments_list:
            if user_comment['text'] == comment:
                break
        else:
            print(f"Wait for '{comment}', but got {user_comment['text']}")
            return Verdict.MUMBLE("incorrect comment")

        user_info = api.api_user_info(token, username)
        if user_info['username'] != username or user_info['id'] != user_dict['id']:
            print(f"Incorrect user_info. Got {user_info['username']}, {user_info['id']}, but wait {username}, {user_dict['id']}")
            return Verdict.MUMBLE("incorrect user info")

        for i in range(0, 5):
            users_list = api.api_get_users(token, i)
            if [u for u in users_list if u['username'] == username and u['id'] == user_dict['id']]:
                break
        else:
            print(f"can't find user via api")
            return Verdict.MUMBLE("can't find user")

        for i in range(0, 5):
            barks_list = api.api_get_last_barks(token, i)
            if [b for b in barks_list if b['id'] == bark_id]:
                break
        else:
            print(f"can't find user via api")
            return Verdict.MUMBLE("can't find user")

        return Verdict.OK()
    except RequestException as e:
        print(f"can't connect due to {e}")
        return Verdict.DOWN("can't connect to host")
    except Exception as e:
        print(e)
        return Verdict.MUMBLE("bad proto")


@checker.define_put(vuln_num=1, vuln_rate=2)
def put(put_request: PutRequest) -> Verdict:
    api = Api(put_request.hostname)
    try:
        username, password = get_random_str(), get_random_str()
        r = api.sing_up(username, password, get_random_str(), get_random_str())
        api.add_bark(username, put_request.flag, is_private=True)
        api.logout()
        return Verdict.OK(f"{username}:{password}")
    except RequestException as e:
        print(f"can't connect due to {e}")
        return Verdict.DOWN("can't connect to host")
    except Exception as e:
        print(e)
        return Verdict.MUMBLE("bad proto")


@checker.define_get(vuln_num=1)
def get(get_request: GetRequest) -> Verdict:
    api = Api(get_request.hostname)
    try:
        username, password = get_request.flag_id.split(":")
        r = api.login(username, password)

        if r.status_code != 200:
            return Verdict.MUMBLE("can't login")

        if get_request.flag in r.text:
            return Verdict.OK()
        else:
            print(f"Can't find flag {get_request.flag} in {r.text}")
            return Verdict.CORRUPT("flag not found")

    except RequestException as e:
        print(f"can't connect due to {e}")
        return Verdict.DOWN("can't connect to host")
    except Exception as e:
        print(e)
        return Verdict.MUMBLE("bad proto")


@checker.define_put(vuln_num=2, vuln_rate=1)
def put(put_request: PutRequest) -> Verdict:
    api = Api(put_request.hostname)
    try:
        username, password, bark = get_random_str(), get_random_str(), get_random_text()
        r = api.sing_up(username, password, get_random_str(), get_random_str())
        r = api.add_bark(username, bark, is_private=False)
        bark_id = int(r.text.split(bark)[0].split("/get_bark/")[1][0:-3])
        api.comment_bark(bark_id, put_request.flag, True)
        api.logout()
        return Verdict.OK(f"{username}:{password}:{bark_id}")
    except RequestException as e:
        print(f"can't connect due to {e}")
        return Verdict.DOWN("can't connect to host")
    except Exception as e:
        print(e)
        return Verdict.MUMBLE("bad proto")


@checker.define_get(vuln_num=2)
def get(get_request: GetRequest) -> Verdict:
    api = Api(get_request.hostname)
    try:
        username, password, bark_id = get_request.flag_id.split(":")
        r = api.login(username, password)

        if r.status_code != 200:
            return Verdict.MUMBLE("can't login")

        r = api.get_bark(bark_id)
        if get_request.flag in r.text:
            return Verdict.OK()
        else:
            print(f"Can't find flag {get_request.flag} in {r.text}")
            return Verdict.CORRUPT("flag not found")

    except RequestException as e:
        print(f"can't connect due to {e}")
        return Verdict.DOWN("can't connect to host")
    except Exception as e:
        print(e)
        return Verdict.MUMBLE("bad proto")


@checker.define_put(vuln_num=3, vuln_rate=2)
def put(put_request: PutRequest) -> Verdict:
    api = Api(put_request.hostname)
    try:
        username, password, bark = get_random_str(), get_random_str(), get_random_text()
        r = api.sing_up(username, password, get_random_str(), get_random_str())
        r = api.add_bark(username, bark, is_private=True)
        bark_id = int(r.text.split(bark)[0].split("/get_bark/")[1][0:-3])
        api.comment_bark(bark_id, put_request.flag, is_private=True)
        token = api.generate_token()
        api.logout()
        return Verdict.OK(f"{username}:{password}:{bark_id}")
    except RequestException as e:
        print(f"can't connect due to {e}")
        return Verdict.DOWN("can't connect to host")
    except Exception as e:
        print(e)
        return Verdict.MUMBLE("bad proto")


@checker.define_get(vuln_num=3)
def get(get_request: GetRequest) -> Verdict:
    api = Api(get_request.hostname)
    try:
        username, password, bark_id = get_request.flag_id.split(":")
        r = api.login(username, password)

        if r.status_code != 200:
            return Verdict.MUMBLE("can't login")

        r = api.get_bark(bark_id)
        if get_request.flag in r.text:
            return Verdict.OK()
        else:
            print(f"Can't find flag {get_request.flag} in {r.text}")
            return Verdict.CORRUPT("flag not found")

    except RequestException as e:
        print(f"can't connect due to {e}")
        return Verdict.DOWN("can't connect to host")
    except Exception as e:
        print(e)
        return Verdict.MUMBLE("bad proto")


checker.run()
