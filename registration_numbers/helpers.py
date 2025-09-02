from typing import Union


def get_nested_value(data: dict, keys: Union[str, list], default=None, previous_key: str = None):
    if isinstance(keys, str):
        keys = keys.split(".")
    if not keys:
        return data

    key = keys[0]
    if isinstance(data, dict):
        if key in data:
            return get_nested_value(data[key], keys[1:], default, key)
        else:
            if isinstance(default, list) or default:
                return default
            # if previous_key is None:
            #     raise KeyError(f"Key '{key}' not found in the current dictionary.")
            # raise KeyError(f"Key '{key}' not found under the dict '{previous_key}'.")
    else:
        return default
