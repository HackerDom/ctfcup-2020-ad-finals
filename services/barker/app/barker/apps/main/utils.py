from .models import Friendship


def get_subscribes(user):
    subscribers = []
    subscribers_fs = Friendship.objects.select_related().filter(first_user=user, status=False)
    for friendship in subscribers_fs:
        subscribers.append(friendship.second_user)
    return subscribers


def get_subscribers(user):
    subscribers = []
    subscribers_fs = Friendship.objects.select_related().filter(second_user=user, status=False)
    for friendship in subscribers_fs:
        subscribers.append(friendship.first_user)
    return subscribers


def get_friends(user):
    friends = []

    second_friends = Friendship.objects.select_related() \
        .filter(first_user=user, status=True)
    for friend in second_friends:
        friends.append(friend.second_user)

    first_friends = Friendship.objects.select_related() \
        .filter(second_user=user, status=True)
    for friend in first_friends:
        friends.append(friend.first_user)

    return friends


def get_user_friends_info(user):
    return {"friends": get_friends(user), "subscribes": get_subscribes(user), "subscribers": get_subscribers(user)}


def is_friends(fuser, suser):
    return Friendship.objects.filter(first_user=fuser, second_user=suser, status=True).first() or \
           Friendship.objects.filter(first_user=suser, second_user=fuser, status=True).first() or \
           fuser == suser
