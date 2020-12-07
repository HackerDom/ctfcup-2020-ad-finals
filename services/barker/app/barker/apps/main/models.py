from django.db import models
from django.contrib.auth.models import User


class Bark(models.Model):
    user = models.ForeignKey(User, on_delete=models.CASCADE)
    text = models.CharField(max_length=256)
    pub_date = models.DateTimeField(auto_now=True)
    is_private = models.BooleanField(default=False)


class Comment(models.Model):
    bark = models.ForeignKey(Bark, on_delete=models.CASCADE)
    user = models.ForeignKey(User, on_delete=models.CASCADE)
    text = models.CharField(max_length=128)
    pub_date = models.DateTimeField(auto_now=True)
    is_private = models.BooleanField(default=False)


class Friendship(models.Model):
    first_user = models.ForeignKey(User, on_delete=models.CASCADE, related_name="fuser")
    second_user = models.ForeignKey(User, on_delete=models.CASCADE, related_name="suser")
    status = models.BooleanField(default=False) # do True for vuln


class Token(models.Model):
    owner = models.ForeignKey(User, on_delete=models.CASCADE)
    value = models.CharField(max_length=32)
