//
//  CommandManager.h
//  InfinarioSDK
//
//  Created by Igi on 2/5/15.
//  Copyright (c) 2015 Infinario. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import "Command.h"

@interface CommandManager : NSObject

- (instancetype)initWithTarget:(NSString *)target andWithToken:(NSString *)token;
- (void)schedule:(Command *)command;
- (void)flush;

@end
